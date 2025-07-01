using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Net.payOS.Types;
using Newtonsoft.Json;
using AppleEnum = AppleShop.Share.Enumerations;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Order
{
    public class ViewPurchaseHistoryHandler : IRequestHandler<ViewPurchaseHistoryRequest, Result<OrderItemListResponseDTO>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IFileService fileService;
        private readonly IPayOSService payOSService;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly ICacheService cacheService;

        public ViewPurchaseHistoryHandler(IOrderRepository orderRepository,
                                          IProductVariantRepository productVariantRepository,
                                          IProductAttributeRepository productAttributeRepository,
                                          IAttributeValueRepository attributeValueRepository,
                                          IProductRepository productRepository,
                                          IProductImageRepository productImageRepository,
                                          IFileService fileService,
                                          IPayOSService payOSService,
                                          IOrderItemRepository orderItemRepository,
                                          IPaymentRepository paymentRepository,
                                          ITransactionRepository transactionRepository,
                                          ICacheService cacheService)
        {
            this.orderRepository = orderRepository;
            this.productVariantRepository = productVariantRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.fileService = fileService;
            this.payOSService = payOSService;
            this.orderItemRepository = orderItemRepository;
            this.paymentRepository = paymentRepository;
            this.transactionRepository = transactionRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<OrderItemListResponseDTO>> Handle(ViewPurchaseHistoryRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"purchase_history_{request.UserId}_{request.Status}_{request.Skip}_{request.Take}";
            var cachedResult = await cacheService.GetAsync<Result<OrderItemListResponseDTO>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var orders = orderRepository.FindAll(x => x.UserId == request.UserId, false, o => o.OrderItems).ToList();
            using var transaction = await orderRepository.BeginTransactionAsync(cancellationToken);

            try
            {
                var paymentMap = new Dictionary<string, string>();
                foreach (var order in orders)
                {
                    await Task.Delay(150);
                    if (order.Payment.ToLower() != Payment.PayOS.ToString().ToLower() || (order.Status != (int)OrderStatus.Pending && order.Status != (int)OrderStatus.Cancelled)) continue;
                    var payment = await payOSService.GetPaymentStatusAsync(order.OrderCode);
                    if (payment.Status == "EXPIRED") paymentMap[order.OrderCode] = "Payment has expired.";
                    if (payment.Status == "PENDING") paymentMap[order.OrderCode] = $"https://pay.payos.vn/web/{payment.Id}";
                    if (payment.Status == "PAID" && order.Status == (int)AppleEnum.OrderStatus.Pending)
                    {
                        var transactionCode = $"PAYOS-{order.OrderCode}-{DateTime.Now:yyyyMMddHHmmss}";

                        var paymentEntity = await paymentRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
                        if (paymentEntity is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Payment));

                        var transactionOrder = await transactionRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
                        if (transactionOrder is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Transaction));

                        order.Status = (int)AppleEnum.OrderStatus.Paid;
                        order.UpdatedAt = DateTime.Now;
                        orderRepository.Update(order);

                        foreach (var item in order.OrderItems)
                        {
                            item.ItemStatus = (int)AppleEnum.ItemStatus.Packed;
                            orderItemRepository.Update(item);
                        }

                        transactionOrder.Status = (int)AppleEnum.TransactionStatus.Success;
                        transactionOrder.Code = transactionCode;
                        transactionOrder.UpdatedAt = DateTime.Now;
                        transactionRepository.Update(transactionOrder);

                        paymentEntity.Status = (int)AppleEnum.PaymentStatus.Success;
                        paymentEntity.TransactionCode = transactionCode;
                        paymentEntity.UpdatedAt = DateTime.Now;
                        paymentRepository.Update(paymentEntity);
                    }
                    if (payment.Status == "CANCELLED" && order.Status == (int)AppleEnum.OrderStatus.Pending)
                    {
                        var transactionCode = $"PAYOS-{order.OrderCode}-{DateTime.Now:yyyyMMddHHmmss}";

                        var paymentEntity = await paymentRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
                        if (paymentEntity is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Payment));

                        var transactionOrder = await transactionRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
                        if (transactionOrder is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Transaction));

                        order.Status = (int)OrderStatus.Cancelled;
                        order.UpdatedAt = DateTime.Now;
                        orderRepository.Update(order);

                        foreach (var item in order.OrderItems)
                        {
                            item.ItemStatus = (int)ItemStatus.Cancelled;
                            var variant = await productVariantRepository.FindByIdAsync(item.VariantId, true);
                            variant.ReservedStock -= item.Quantity;

                            productVariantRepository.Update(variant);
                            orderItemRepository.Update(item);
                        }

                        transactionOrder.Status = (int)AppleEnum.TransactionStatus.Failed;
                        transactionOrder.Code = transactionCode;
                        transactionOrder.UpdatedAt = DateTime.Now;
                        transactionRepository.Update(transactionOrder);

                        paymentEntity.Status = (int)PaymentStatus.Failed;
                        paymentEntity.TransactionCode = transactionCode;
                        paymentEntity.UpdatedAt = DateTime.Now;
                        paymentRepository.Update(paymentEntity);

                        await productVariantRepository.SaveChangesAsync(cancellationToken);
                    }
                    if (payment.Status == "EXPIRED" && order.Status == (int)AppleEnum.OrderStatus.Pending)
                    {
                        var transactionCode = $"PAYOS-{order.OrderCode}-{DateTime.Now:yyyyMMddHHmmss}";

                        var paymentEntity = await paymentRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
                        if (paymentEntity is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Payment));

                        var transactionOrder = await transactionRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
                        if (transactionOrder is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Transaction));

                        order.Status = (int)OrderStatus.Cancelled;
                        order.UpdatedAt = DateTime.Now;
                        orderRepository.Update(order);

                        foreach (var item in order.OrderItems)
                        {
                            item.ItemStatus = (int)ItemStatus.Cancelled;
                            var variant = await productVariantRepository.FindByIdAsync(item.VariantId, true);
                            variant.ReservedStock -= item.Quantity;

                            productVariantRepository.Update(variant);
                            orderItemRepository.Update(item);
                        }

                        transactionOrder.Status = (int)AppleEnum.TransactionStatus.Failed;
                        transactionOrder.Code = transactionCode;
                        transactionOrder.UpdatedAt = DateTime.Now;
                        transactionRepository.Update(transactionOrder);

                        paymentEntity.Status = (int)PaymentStatus.Failed;
                        paymentEntity.TransactionCode = transactionCode;
                        paymentEntity.UpdatedAt = DateTime.Now;
                        paymentRepository.Update(paymentEntity);

                        await productVariantRepository.SaveChangesAsync(cancellationToken);
                    }
                }

                var variantIds = orders.SelectMany(o => o.OrderItems).Select(x => x.VariantId).ToList();
                if (variantIds is null || !variantIds.Any()) AppleException.ThrowNotFound(message: "No products found in the order.");

                var variants = productVariantRepository.FindAll(x => variantIds.Contains(x.Id)).ToDictionary(p => p.Id, p => p);
                var products = productRepository.FindAll(x => variants.Select(p => p.Value.ProductId).Contains(x.Id)).ToDictionary(p => p.Id, p => p);
                var productImage = productImageRepository
                                    .FindAll(x => variants.Select(v => v.Value.Id).Contains(x.VariantId) && x.Position == 0)
                                    .GroupBy(pi => pi.VariantId)
                                    .ToDictionary(g => g.Key, g => g.First().Url);

                var productAttributes = productAttributeRepository.FindAll(x => variantIds.Contains(x.VariantId)).ToList();
                var attributeValues = attributeValueRepository.FindAll().ToDictionary(av => av.Id, av => av.Value);

                var orderItemDtos = orders.SelectMany(o => o.OrderItems)
                    .Where(oi => oi.ItemStatus != (int)ItemStatus.PendingReturn && oi.ItemStatus != (int)ItemStatus.RejectedReturn && oi.ItemStatus != (int)ItemStatus.ApprovedReturn
                                && (request.Status is null || oi.ItemStatus == request.Status))
                    .Select(oi => new OrderItemFullDTO
                    {
                        OiId = oi.Id,
                        VariantId = oi.VariantId ?? 0,
                        Status = ((ItemStatus)oi.ItemStatus).ToString(),
                        Name = variants.ContainsKey(oi.VariantId ?? 0) && products.ContainsKey(variants[oi.VariantId.Value].ProductId)
                                      ? $"{products[variants[oi.VariantId.Value].ProductId].Name} - {string.Join(" ", productAttributes.Where(pa => pa.VariantId == oi.VariantId).Select(pa => attributeValues.TryGetValue(pa.AvId, out var value) ? value : "Unknown"))}"
                                      : null,
                        Image = productImage.TryGetValue(oi.VariantId ?? 0, out var imageUrl)
                                        ? fileService.GetFullPathFileServer(imageUrl)
                                        : null,
                        ProductAttribute = string.Join(" - ", productAttributes
                                          .Where(pa => pa.VariantId == oi.VariantId)
                                          .Select(pa => attributeValues.TryGetValue(pa.AvId, out var value) ? value : "Unknown")),
                        Quantity = oi.Quantity ?? 0,
                        OriginalPrice = oi.OriginalPrice ?? 0,
                        FinalPrice = oi.FinalPrice ?? 0,
                        TotalPrice = oi.TotalPrice ?? 0,
                        PaymentUrl = orders.FirstOrDefault(o => o.OrderItems.Any(x => x.Id == oi.Id)) is { } order
                                && paymentMap.TryGetValue(order.OrderCode, out var url)
                                    ? url : null,
                        IsReview = oi.IsReview != false ? oi.IsReview : null,
                    }).OrderByDescending(x => x.OiId).ToList();

                var totalItems = orderItemDtos.Count();
                var pagedDtos = orderItemDtos.Skip(request.Skip ?? 0).Take(request.Take ?? totalItems).ToList();
                var result = Result<OrderItemListResponseDTO>.Ok(new OrderItemListResponseDTO { TotalItems = totalItems, Items = pagedDtos });

                await orderRepository.SaveChangesAsync(cancellationToken);
                await orderItemRepository.SaveChangesAsync(cancellationToken);
                await transactionRepository.SaveChangesAsync(cancellationToken);
                await paymentRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                return result;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}