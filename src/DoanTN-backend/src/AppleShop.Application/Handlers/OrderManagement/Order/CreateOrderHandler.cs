using AppleShop.Application.Handlers.OrderManagement.Sagas;
using AppleShop.Application.Requests.DTOs.OrderManagement.Saga;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Application.Services;
using AppleShop.Application.Validators.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.AIManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Service;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.OrderManagement.Order
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderRequest, Result<OrderSagaDTO>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly ICouponRepository couponRepository;
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IUserCouponRepository userCouponRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IPayOSService vnPayService;
        private readonly IProductRepository productRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly IUserInteractionRepository userInteractionRepository;
        private readonly IEmailService emailService;
        private readonly IUserRepository userRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly ICacheService cacheService;
        private readonly INotificationService notificationService;

        public CreateOrderHandler(IOrderRepository orderRepository,
                                IOrderItemRepository orderItemRepository,
                                ICouponRepository couponRepository,
                                ICouponTypeRepository couponTypeRepository,
                                IUserCouponRepository userCouponRepository,
                                IProductVariantRepository productVariantRepository,
                                IPromotionRepository promotionRepository,
                                IProductPromotionRepository productPromotionRepository,
                                ITransactionRepository transactionRepository,
                                IPaymentRepository paymentRepository,
                                IPayOSService vnPayService,
                                IProductRepository productRepository,
                                IProductAttributeRepository productAttributeRepository,
                                IAttributeValueRepository attributeValueRepository,
                                IAttributeRepository attributeRepository,
                                IUserInteractionRepository userInteractionRepository,
                                IEmailService emailService,
                                IUserRepository userRepository,
                                IProductImageRepository productImageRepository,
                                ICacheService cacheService,
                                INotificationService notificationService)
        {
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.couponRepository = couponRepository;
            this.couponTypeRepository = couponTypeRepository;
            this.userCouponRepository = userCouponRepository;
            this.productVariantRepository = productVariantRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.transactionRepository = transactionRepository;
            this.paymentRepository = paymentRepository;
            this.vnPayService = vnPayService;
            this.productRepository = productRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.attributeRepository = attributeRepository;
            this.userInteractionRepository = userInteractionRepository;
            this.emailService = emailService;
            this.userRepository = userRepository;
            this.productImageRepository = productImageRepository;
            this.cacheService = cacheService;
            this.notificationService = notificationService;
        }

        public async Task<Result<OrderSagaDTO>> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateOrderValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            using var transaction = await orderRepository.BeginTransactionAsync(cancellationToken);

            var orderSagaDto = new OrderSagaDTO();
            var saga = new SagaOrchestrator();
            saga.AddStep(new CreateOrderStep(orderRepository, orderItemRepository, productVariantRepository, request, promotionRepository, productPromotionRepository, orderSagaDto));
            saga.AddStep(new ApplyVoucherStep(couponRepository, couponTypeRepository, userCouponRepository, orderRepository, orderItemRepository, request, orderSagaDto));
            saga.AddStep(new ProcessPaymentStep(orderRepository, transactionRepository, request, orderSagaDto));
            saga.AddStep(new PayOSPaymentStep(vnPayService, orderRepository, request, orderSagaDto, productVariantRepository, productRepository, productAttributeRepository, attributeValueRepository));
            saga.AddStep(new LogPayOSTransactionStep(paymentRepository, transactionRepository, orderRepository, request, orderSagaDto));

            try
            {
                var result = await saga.ExecuteAsync(cancellationToken);
                foreach (var item in request.OrderItems)
                {
                    var als = new UserInteraction
                    {
                        UserId = request.UserId,
                        VariantId = item.VariantId,
                        Type = "order",
                        Value = 5,
                        CreatedAt = DateTime.Now,
                    };
                    userInteractionRepository.Create(als);

                    await cacheService.RemoveAsync($"product_detail_{item.VariantId}");
                }

                await userInteractionRepository.SaveChangesAsync(cancellationToken);
                await cacheService.RemoveByPatternAsync($"purchase_history_*");
                await cacheService.RemoveByPatternAsync("product_variants_*");

                var order = await orderRepository.FindSingleAsync(x => x.OrderCode == orderSagaDto.Code, false);
                var user = await userRepository.FindByIdAsync(request.UserId ?? 0, false);
                var orderItems = orderItemRepository.FindAll(x => x.OrderId == order.Id).ToList();
                var variants = productVariantRepository.FindAll(x => orderItems.Select(x => x.VariantId).Contains(x.Id)).ToDictionary(p => p.Id, p => p);
                var products = productRepository.FindAll(x => variants.Select(p => p.Value.ProductId).Contains(x.Id)).ToDictionary(p => p.Id, p => p);
                var productAttributes = productAttributeRepository.FindAll(x => variants.Select(p => p.Value.Id).Contains(x.VariantId)).ToList();
                var attributeValues = attributeValueRepository.FindAll(x => productAttributes.Select(x => x.AvId).Contains(x.Id)).ToList();
                var attributes = attributeRepository.FindAll(x => true).ToDictionary(attr => attr.Id, attr => attr.Name);
                var productImages = productImageRepository.FindAll(x => variants.Select(p => p.Value.ProductId).Contains(x.ProductId) && x.Position == 0).ToDictionary(p => p.ProductId, p => p.Url);

                string[] desiredOrder = { "kích thước", "màu sắc", "ram", "dung lượng", "cổng sạc" };

                var orderItemsHtml = string.Join("", orderItems.Select(oi =>
                {
                    var fullAttributes = (from pa in productAttributes
                                          where pa.VariantId == oi.VariantId
                                          join av in attributeValues on pa.AvId equals av.Id
                                          join attr in attributes on av.AttributeId equals attr.Key
                                          select new
                                          {
                                              AttributeName = attr.Value,
                                              AttributeValue = av.Value
                                          }).ToList();

                    var orderedAttributes = fullAttributes
                                            .OrderBy(attr =>
                                            {
                                                var lowerName = attr.AttributeName.ToLower();
                                                for (int i = 0; i < desiredOrder.Length; i++)
                                                {
                                                    if (lowerName.Contains(desiredOrder[i])) return i;
                                                }
                                                return desiredOrder.Length;
                                            }).ToList();

                    var productName = variants.ContainsKey(oi.VariantId ?? 0)
                        ? $"{products[variants[oi.VariantId.Value].ProductId].Name} - {string.Join(" ", orderedAttributes.Select(a => a.AttributeValue))}"
                        : "Unknown Product";

                    var productId = variants.ContainsKey(oi.VariantId ?? 0) ? variants[oi.VariantId.Value].ProductId : 0;
                    var productImage = productImages.ContainsKey(productId) ? productImages[productId] : "";

                    return $@"
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>
                                <div style='display: flex; align-items: center; gap: 10px;'>
                                    <img src='{productImage}' alt='{productName}' style='width: 80px; height: 80px; object-fit: cover; border-radius: 4px;' />
                                    <div>{productName}</div>
                                </div>
                            </td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{oi.Quantity}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{oi.FinalPrice:N0} VNĐ</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{oi.TotalPrice:N0} VNĐ</td>
                        </tr>";
                }));

                var subject = "Cảm ơn bạn đã đặt hàng tại Hưng AppleShop!";
                var body = $@"<div style='font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; color: #333;'>
                    <div style='padding: 20px; border-bottom: 3px solid #007BFF; text-align: center;'>
                        <img src='https://drive.google.com/uc?export=view&id=1TLWTKoXTzjte2wyn0jnHjTrGUGdcbv98' alt='Logo'
                            style='width: 150px; margin-bottom: 10px;' />
                        <h2 style='color: #007BFF; font-weight: bold; margin: 0;'>Cảm ơn bạn đã đặt hàng!</h2>
                    </div>
                    <div style='padding: 20px;'>
                        <p>Xin chào {user?.Username},</p>
                        <p>Cảm ơn bạn đã đặt hàng tại Hưng AppleShop. Dưới đây là thông tin chi tiết đơn hàng của bạn:</p>
                        
                        <div style='margin: 20px 0;'>
                            <p><strong>Mã đơn hàng:</strong> {order.OrderCode}</p>
                            <p><strong>Ngày đặt hàng:</strong> {order.CreatedAt:dd/MM/yyyy HH:mm}</p>
                        </div>

                        <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                            <thead>
                                <tr style='background-color: #f8f9fa;'>
                                    <th style='padding: 10px; text-align: left; border-bottom: 2px solid #ddd;'>Sản phẩm</th>
                                    <th style='padding: 10px; text-align: center; border-bottom: 2px solid #ddd;'>Số lượng</th>
                                    <th style='padding: 10px; text-align: right; border-bottom: 2px solid #ddd;'>Đơn giá</th>
                                    <th style='padding: 10px; text-align: right; border-bottom: 2px solid #ddd;'>Thành tiền</th>
                                </tr>
                            </thead>
                            <tbody>
                                {orderItemsHtml}
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td colspan='3' style='padding: 10px; text-align: right;'><strong>Phí vận chuyển:</strong></td>
                                    <td style='padding: 10px; text-align: right;'>{order.ShipFee:N0} VNĐ</td>
                                </tr>
                                <tr>
                                    <td colspan='3' style='padding: 10px; text-align: right;'><strong>Tổng tiền:</strong></td>
                                    <td style='padding: 10px; text-align: right;'><strong>{order.TotalAmount:N0} VNĐ</strong></td>
                                </tr>
                            </tfoot>
                        </table>

                        <p>Chúng tôi sẽ xử lý đơn hàng của bạn trong thời gian sớm nhất. Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>
                        <p>Trân trọng,<br />Hưng AppleShop!</p>
                    </div>
                    <div style='background-color: #343a40; color: white; padding: 10px; text-align: center; font-size: 12px; border-top: 3px solid #007BFF;'>
                        © 2024 Hưng AppleShop
                    </div>
                </div>";

                await emailService.SendEmailAsync(user?.Email, subject, body);

                // Thông báo cho admin khi có đơn hàng mới
                await notificationService.NotifyNewOrder(order.Id.Value, order.OrderCode);

                transaction.Commit();
                return Result<OrderSagaDTO>.Ok(orderSagaDto);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}