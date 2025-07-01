using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.DTOs.WishListManagement.WishList;
using AppleShop.Application.Requests.WishListManagement.WishList;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Abstractions.IRepositories.WishListManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.WishListManagement.WishList
{
    public class GetDetailWishListHandler : IRequestHandler<GetDetailWishListRequest, Result<List<WishListFullDTO>>>
    {
        private readonly IWishListRepository wishListRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IFileService fileService;
        private readonly ICacheService cacheService;

        public GetDetailWishListHandler(IWishListRepository wishListRepository,
                                        IUserRepository userRepository,
                                        IProductVariantRepository productVariantRepository,
                                        IProductPromotionRepository productPromotionRepository,
                                        IProductAttributeRepository productAttributeRepository,
                                        IAttributeValueRepository attributeValueRepository,
                                        IPromotionRepository promotionRepository,
                                        IProductRepository productRepository,
                                        IProductImageRepository productImageRepository,
                                        IFileService fileService,
                                        ICacheService cacheService)
        {
            this.wishListRepository = wishListRepository;
            this.userRepository = userRepository;
            this.productVariantRepository = productVariantRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.promotionRepository = promotionRepository;
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.fileService = fileService;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<WishListFullDTO>>> Handle(GetDetailWishListRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"wish_list_{request.UserId}";
            var cachedResult = await cacheService.GetAsync<Result<List<WishListFullDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var user = await userRepository.FindSingleAsync(x => x.Id == request.UserId && x.IsActived == 1);
            if (user is null) AppleException.ThrowUnAuthorization("Please log in.");

            var query = wishListRepository.FindAll(x => x.UserId == request.UserId);
            if (request.IsActived is not null) query = query.Where(x => x.IsActived ==  request.IsActived);

            var wishList = query.ToList();

            var variants = productVariantRepository.FindAll(x => wishList.Select(x => x.VariantId).Contains(x.Id)).ToDictionary(p => p.Id);

            var productDict = productRepository.FindAll(x => variants.Select(p => p.Value.ProductId).Contains(x.Id)).ToDictionary(p => p.Id);

            var productImage = await productImageRepository.FindSingleAsync(x => variants.Select(v => v.Value.Id).Contains(x.VariantId) && x.Position == 0);

            var productAttributes = productAttributeRepository.FindAll(x => variants.Select(x => x.Value.Id).Contains(x.VariantId)).ToList();
            var attributeValues = attributeValueRepository.FindAll().ToList();
            var attributeValueDict = attributeValues.ToDictionary(av => av.Id, av => av.Value);

            var promotions = promotionRepository.FindAll(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActived == 1).ToList();
            var productPromotions = productPromotionRepository.FindAll().ToList();

            var wishListDtos = wishList.Select(wl => new WishListFullDTO
            {
                Id = wl.Id,
                VariantId = wl.VariantId,
                Name = variants.ContainsKey(wl.VariantId ?? 0)
                                  ? $"{productDict[variants[wl.VariantId.Value].ProductId].Name} - {string.Join(" ", productAttributes.Where(pa => pa.VariantId == wl.VariantId).Select(pa => attributeValueDict.TryGetValue(pa.AvId, out var value) ? value : "Unknown"))}"
                                  : null,
                Image = fileService.GetFullPathFileServer(productImage.Url),
                Description = variants.ContainsKey(wl.VariantId ?? 0) ? productDict[variants[wl.VariantId.Value].ProductId].Description : null,
                UnitPrice = variants.ContainsKey(wl.VariantId ?? 0) ? variants[wl.VariantId.Value].Price : 0,
                DiscountPrice = variants.ContainsKey(wl.VariantId ?? 0) ? GetDiscountedPrice(variants[wl.VariantId.Value], promotions, productPromotions) : 0,
                ProductAttributes = productAttributes.Where(pa => pa.VariantId == wl.VariantId)
                                                         .Select(pa => attributeValueDict.TryGetValue(pa.AvId, out var value) ? value : "Unknown")
                                                         .ToList()
            }).ToList();

            var result = Result<List<WishListFullDTO>>.Ok(wishListDtos);
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