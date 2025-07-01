using AppleShop.Application.Requests.DTOs.OrderManagement.Saga;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using System.Text.RegularExpressions;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Sagas
{
    public class CreateOrderStep : ISagaStep
    {
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly CreateOrderRequest request;
        private readonly OrderSagaDTO orderSagaDTO;

        public Entities.OrderManagement.Order? CreatedOrder { get; private set; }
        public List<OrderItem> CreatedOrderItems { get; private set; } = new();

        public CreateOrderStep(IOrderRepository orderRepository,
                               IOrderItemRepository orderItemRepository,
                               IProductVariantRepository productVariantRepository,
                               CreateOrderRequest request,
                               IPromotionRepository promotionRepository,
                               IProductPromotionRepository productPromotionRepository,
                               OrderSagaDTO orderSagaDTO)
        {
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.productVariantRepository = productVariantRepository;
            this.request = request;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.orderSagaDTO = orderSagaDTO;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var guid = Guid.NewGuid().ToString();
            var orderCode = Regex.Replace(guid, "[^0-9]", "").Substring(0, 10).ToUpper();

            var order = new Entities.OrderManagement.Order
            {
                OrderCode = orderCode,
                Status = (int)OrderStatus.Pending,
                Payment = request.Payment,
                UserId = request.UserId,
                UserAddressId = request.UserAddressId,
                CreatedAt = DateTime.Now,
                TotalAmount = 0,
                ShipFee = 30000
            };

            orderRepository.Create(order);
            await orderRepository.SaveChangesAsync(cancellationToken);

            var productVariants = productVariantRepository.FindAll(x => request.OrderItems.Select(i => i.VariantId).Contains(x.Id)).ToDictionary(x => x.Id);

            foreach (var item in request.OrderItems)
            {
                var variant = productVariants[item.VariantId];
                int availableStock = variant.Stock.Value - variant.ReservedStock.Value;
                if (availableStock < item.Quantity) AppleException.ThrowConflict("Not enough stock available.");

                decimal finalPrice = GetDiscountedPrice(variant, promotionRepository, productPromotionRepository);
                decimal totalItemPrice = finalPrice * item.Quantity.Value;

                variant.ReservedStock += item.Quantity;
                productVariantRepository.Update(variant);

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    OriginalPrice = finalPrice,
                    FinalPrice = finalPrice,
                    TotalPrice = totalItemPrice,
                    ItemStatus = (int)ItemStatus.Pending,
                    IsReview = false,
                };

                orderItemRepository.Create(orderItem);
                CreatedOrderItems.Add(orderItem);
            }

            await productVariantRepository.SaveChangesAsync(cancellationToken);
            await orderItemRepository.SaveChangesAsync(cancellationToken);
            await orderRepository.ExecuteSqlRawAsync("EXEC sp_UpdateOrderStatus @OrderId = {0}", order.Id);

            CreatedOrder = order;
            orderSagaDTO.Code = order.OrderCode;
            orderSagaDTO.Amount = order.TotalAmount;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            CreatedOrder.Status = (int)OrderStatus.Cancelled;
            orderRepository.Update(CreatedOrder);
            await orderRepository.SaveChangesAsync(cancellationToken);
        }

        private decimal GetDiscountedPrice(Entities.ProductManagement.ProductVariant productVariant, IPromotionRepository promotionRepository, IProductPromotionRepository productPromotionRepository)
        {
            var promotions = promotionRepository.FindAll(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActived == 1).ToList();
            var productPromotions = productPromotionRepository.FindAll().ToList();

            var discountPercentage = 0;
            var discountAmount = 0;

            var variantPromotions = productPromotions.Where(pp => pp.VariantId == productVariant.Id).ToList();
            if (variantPromotions.Any())
            {
                foreach (var pp in variantPromotions)
                {
                    var promo = promotions.FirstOrDefault(p => p.Id == pp.PromotionId);
                    if (promo != null)
                    {
                        discountPercentage = Math.Max(discountPercentage, promo.DiscountPercentage ?? 0);
                        discountAmount = Math.Max(discountAmount, promo.DiscountAmout ?? 0);
                    }
                }
            }

            var productPromotionsForProduct = productPromotions.Where(pp => pp.ProductId == productVariant.ProductId).ToList();
            if (productPromotionsForProduct.Any())
            {
                foreach (var pp in productPromotionsForProduct)
                {
                    var promo = promotions.FirstOrDefault(p => p.Id == pp.PromotionId);
                    if (promo != null)
                    {
                        discountPercentage = Math.Max(discountPercentage, promo.DiscountPercentage ?? 0);
                        discountAmount = Math.Max(discountAmount, promo.DiscountAmout ?? 0);
                    }
                }
            }

            var finalDiscount = discountPercentage > 0 ? productVariant.Price * discountPercentage / 100 : discountAmount;
            return Math.Max((decimal)(productVariant.Price - finalDiscount), 0);
        }
    }
}