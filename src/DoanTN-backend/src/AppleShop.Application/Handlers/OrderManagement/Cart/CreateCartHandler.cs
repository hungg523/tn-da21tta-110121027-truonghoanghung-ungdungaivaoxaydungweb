using AppleShop.Application.Requests.OrderManagement.Cart;
using AppleShop.Application.Validators.OrderManagement.Cart;
using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Entities.AIManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Cart
{
    public class CreateCartHandler : IRequestHandler<CreateCartRequest, Result<object>>
    {
        private readonly ICartRepository cartRepository;
        private readonly IMapper mapper;
        private readonly ICartItemRepository cartItemRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IUserInteractionRepository userInteractionRepository;
        private readonly ICacheService cacheService;

        public CreateCartHandler(ICartRepository cartRepository, IMapper mapper, ICartItemRepository cartItemRepository, IProductVariantRepository productVariantRepository, IPromotionRepository promotionRepository, IProductPromotionRepository productPromotionRepository, IUserInteractionRepository userInteractionRepository, ICacheService cacheService)
        {
            this.cartRepository = cartRepository;
            this.mapper = mapper;
            this.cartItemRepository = cartItemRepository;
            this.productVariantRepository = productVariantRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.userInteractionRepository = userInteractionRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateCartRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateCartValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            using var transaction = await cartRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var cart = await cartRepository.FindSingleAsync(x => x.UserId == request.UserId, true);
                if (cart is null)
                {
                    cart = new Entities.OrderManagement.Cart
                    {
                        UserId = request.UserId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    cartRepository.Create(cart);
                    await cartRepository.SaveChangesAsync(cancellationToken);
                }

                var existingCartItem = await cartItemRepository.FindSingleAsync(x => x.CartId == cart.Id && x.VariantId == request.VariantId, true);

                var productVariant = await productVariantRepository.FindByIdAsync(request.VariantId, true);
                if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

                var discountPrice = GetDiscountedPrice(productVariant, promotionRepository, productPromotionRepository);

                if (existingCartItem is not null)
                {
                    cart.UpdatedAt = DateTime.Now;
                    cartRepository.Update(cart);
                    await cartRepository.SaveChangesAsync(cancellationToken);

                    existingCartItem.Quantity += request.Quantity ?? 0;
                    existingCartItem.TotalPrice = existingCartItem.Quantity * discountPrice;
                    cartItemRepository.Update(existingCartItem);

                    var als = new UserInteraction
                    {
                        UserId = request.UserId,
                        VariantId = existingCartItem.VariantId,
                        Type = "add_to_cart",
                        Value = 3,
                        CreatedAt = DateTime.Now,
                    };
                    userInteractionRepository.Create(als);
                }
                else
                {
                    var cartItem = new Entities.OrderManagement.CartItem
                    {
                        CartId = cart.Id,
                        VariantId = productVariant.Id,
                        Quantity = request.Quantity ?? 1,
                        UnitPrice = discountPrice,
                        TotalPrice = (request.Quantity ?? 1) * discountPrice
                    };
                    cartItemRepository.Create(cartItem);

                    var als = new UserInteraction
                    {
                        UserId = request.UserId,
                        VariantId = productVariant.Id,
                        Type = "add_to_cart",
                        Value = 3,
                        CreatedAt = DateTime.Now,
                    };
                    userInteractionRepository.Create(als);
                }

                await userInteractionRepository.SaveChangesAsync(cancellationToken);
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