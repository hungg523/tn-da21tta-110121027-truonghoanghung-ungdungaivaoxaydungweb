using AppleShop.Application.Requests.OrderManagement.Cart;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Cart
{
    public class UpdateCartQuantityHandler : IRequestHandler<UpdateCartQuantityRequest, Result<object>>
    {
        private readonly ICartRepository cartRepository;
        private readonly ICartItemRepository cartItemRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly ICacheService cacheService;

        public UpdateCartQuantityHandler(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductVariantRepository productVariantRepository, IPromotionRepository promotionRepository, IProductPromotionRepository productPromotionRepository, ICacheService cacheService)
        {
            this.cartRepository = cartRepository;
            this.cartItemRepository = cartItemRepository;
            this.productVariantRepository = productVariantRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(UpdateCartQuantityRequest request, CancellationToken cancellationToken)
        {
            var cart = await cartRepository.FindSingleAsync(x => x.UserId == request.UserId, true);
            if (cart is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Cart));

            var existingCartItem = await cartItemRepository.FindSingleAsync(x => x.CartId == cart.Id && x.VariantId == request.VariantId, true);

            var productVariant = await productVariantRepository.FindByIdAsync(request.VariantId, true);
            if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

            using var transaction = await cartRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var discountPrice = GetDiscountedPrice(productVariant, promotionRepository, productPromotionRepository);

                cart.UpdatedAt = DateTime.Now;
                cartRepository.Update(cart);
                await cartRepository.SaveChangesAsync(cancellationToken);

                existingCartItem.Quantity = request.Quantity;
                existingCartItem.TotalPrice = request.Quantity * discountPrice;
                cartItemRepository.Update(existingCartItem);

                await cartItemRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.RemoveAsync($"carts_{request.UserId}");
                return Result<object>.Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
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