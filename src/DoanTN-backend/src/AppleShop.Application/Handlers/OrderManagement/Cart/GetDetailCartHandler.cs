using AppleShop.Application.Requests.DTOs.OrderManagement.Cart;
using AppleShop.Application.Requests.OrderManagement.Cart;
using AppleShop.Application.Validators.OrderManagement.Cart;
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
    public class GetDetailCartHandler : IRequestHandler<GetDetailCartRequest, Result<CartFullDTO>>
    {
        private readonly ICartRepository cartRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IFileService fileService;
        private readonly IAttributeRepository attributeRepository;
        private readonly ICacheService cacheService;

        public GetDetailCartHandler(ICartRepository cartRepository,
                                    IProductVariantRepository productVariantRepository,
                                    IProductPromotionRepository productPromotionRepository,
                                    IProductAttributeRepository productAttributeRepository,
                                    IAttributeValueRepository attributeValueRepository,
                                    IPromotionRepository promotionRepository,
                                    IProductRepository productRepository,
                                    IProductImageRepository productImageRepository,
                                    IFileService fileService,
                                    IAttributeRepository attributeRepository,
                                    ICacheService cacheService)
        {
            this.cartRepository = cartRepository;
            this.productVariantRepository = productVariantRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.promotionRepository = promotionRepository;
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.fileService = fileService;
            this.attributeRepository = attributeRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<CartFullDTO>> Handle(GetDetailCartRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailCartValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var cacheKey = $"carts_{request.UserId}";
            var cachedResult = await cacheService.GetAsync<Result<CartFullDTO>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var cart = await cartRepository.FindSingleAsync(x => x.UserId == request.UserId, false, includeProperties: c => c.CartItems);
            if (cart is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Cart));

            var variantIds = cart.CartItems.Select(x => x.VariantId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
            if (variantIds is null || !variantIds.Any()) AppleException.ThrowNotFound(message: "No products found in the cart.");

            var variants = productVariantRepository.FindAll(x => variantIds.Contains(x.Id.Value)).ToDictionary(p => p.Id, p => p);

            var productIds = variants.Select(p => p.Value.ProductId).Distinct().ToList();
            var productDict = productRepository.FindAll(x => productIds.Contains(x.Id)).ToDictionary(p => p.Id, p => p);

            var productImage = productImageRepository.FindAll(x => variantIds.Contains(x.VariantId.Value) && x.Position == 0)
                                    .GroupBy(pi => pi.VariantId)
                                    .ToDictionary(g => g.Key, g => g.First().Url);

            var productAttributes = productAttributeRepository.FindAll(x => variantIds.Contains(x.VariantId.Value)).ToList();
            var attributeValues = attributeValueRepository.FindAll().ToList();
            var attributeValueDict = attributeValues.ToDictionary(av => av.Id, av => av.Value);

            var promotions = promotionRepository.FindAll(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActived == 1).ToList();
            var productPromotions = productPromotionRepository.FindAll().ToList();

            string[] desiredOrder = { "kích thước", "màu sắc", "ram", "dung lượng", "cổng sạc" };

            var attributeRepositoryAll = attributeRepository.FindAll().ToList();

            var variantAttributesDict = productAttributes
                .GroupBy(pa => pa.VariantId)
                .ToDictionary(g => g.Key, g => g.ToList());

            Func<int, string> GetVariantName = variantId =>
            {
                if (!variantAttributesDict.TryGetValue(variantId, out var variantAttrs))
                    return null;

                var fullAttributes = (from pa in variantAttrs
                                      join av in attributeValues on pa.AvId equals av.Id
                                      join attr in attributeRepositoryAll on av.AttributeId equals attr.Id
                                      select new
                                      {
                                          AttributeName = attr.Name,
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
                    })
                    .Select(a => a.AttributeValue)
                    .ToList();

                var baseName = productDict[variants[variantId].ProductId].Name;

                return baseName + " " + string.Join(" ", orderedAttributes);
            };

            var cartDto = new CartFullDTO
            {
                Id = cart.Id,
                CartItems = cart.CartItems?.Select(ci => new CartItemFullDTO
                {
                    VariantId = ci.VariantId ?? 0,
                    Name = variants.ContainsKey(ci.VariantId ?? 0) ? GetVariantName(ci.VariantId.Value) : null,
                    Image = productImage.TryGetValue(ci.VariantId ?? 0, out var imageUrl)
                                        ? fileService.GetFullPathFileServer(imageUrl)
                                        : null,
                    Description = variants.ContainsKey(ci.VariantId ?? 0) ? productDict[variants[ci.VariantId.Value].ProductId].Description : null,
                    Quantity = ci.Quantity ?? 0,

                    UnitPrice = variants.ContainsKey(ci.VariantId ?? 0) ?
                                variants[ci.VariantId.Value].Price : 0,

                    DiscountPrice = variants.ContainsKey(ci.VariantId ?? 0) ?
                                    GetDiscountedPrice(variants[ci.VariantId.Value], promotions, productPromotions) : 0,

                    TotalPrice = (variants.ContainsKey(ci.VariantId ?? 0) ?
                                  GetDiscountedPrice(variants[ci.VariantId.Value], promotions, productPromotions) : variants[ci.VariantId.Value].Price)
                                  * (ci.Quantity ?? 0),

                    ProductAttributes = productAttributes.Where(pa => pa.VariantId == ci.VariantId)
                                                         .Select(pa => attributeValueDict.TryGetValue(pa.AvId, out var value) ? value : "Unknown")
                                                         .ToList()

                }).ToList(),
                TotalPrice = cart.CartItems?.Sum(ci => (variants.ContainsKey(ci.VariantId ?? 0) ?
                                                        GetDiscountedPrice(variants[ci.VariantId.Value], promotions, productPromotions) : variants[ci.VariantId.Value].Price)
                                                        * (ci.Quantity ?? 0)) ?? 0
            };

            var result = Result<CartFullDTO>.Ok(cartDto);
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }

        private decimal GetDiscountedPrice(Entities.ProductManagement.ProductVariant productVariant, List<Entities.PromotionManagement.Promotion> promotions, List<Entities.PromotionManagement.ProductPromotion> allProductPromotions)
        {
            var discountPercentage = 0;
            var discountAmount = 0;

            var variantPromotions = allProductPromotions.Where(pp => pp.VariantId == productVariant.Id).ToList();
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

            var productPromotions = allProductPromotions.Where(pp => pp.ProductId == productVariant.ProductId).ToList();
            if (productPromotions.Any())
            {
                foreach (var pp in productPromotions)
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