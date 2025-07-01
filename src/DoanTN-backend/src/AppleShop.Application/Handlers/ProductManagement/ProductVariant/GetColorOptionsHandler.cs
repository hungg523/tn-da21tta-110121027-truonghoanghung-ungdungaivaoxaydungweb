using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = AppleShop.Domain.Entities.ProductManagement;

namespace AppleShop.Application.Handlers.ProductManagement.ProductVariant
{
    public class GetColorOptionsHandler : IRequestHandler<GetColorOptionsRequest, Result<List<ProductAttributeFullDTO>>>
    {
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IFileService fileService;
        private readonly IProductImageRepository productImageRepository;
        private readonly ICacheService cacheService;

        public GetColorOptionsHandler(IProductAttributeRepository productAttributeRepository,
                                    IProductVariantRepository productVariantRepository,
                                    IAttributeValueRepository attributeValueRepository,
                                    IAttributeRepository attributeRepository,
                                    IPromotionRepository promotionRepository,
                                    IProductPromotionRepository productPromotionRepository,
                                    IFileService fileService,
                                    IProductImageRepository productImageRepository,
                                    ICacheService cacheService)
        {
            this.productAttributeRepository = productAttributeRepository;
            this.productVariantRepository = productVariantRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.attributeRepository = attributeRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.fileService = fileService;
            this.productImageRepository = productImageRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<ProductAttributeFullDTO>>> Handle(GetColorOptionsRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"color_options_{request.VariantId}";
            var cachedResult = await cacheService.GetAsync<Result<List<ProductAttributeFullDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var currentAttributes = productAttributeRepository.FindAll(x => x.VariantId == request.VariantId).ToList();
            var avIds = currentAttributes.Select(x => x.AvId).ToList();

            var variant = await productVariantRepository.FindByIdAsync(request.VariantId);

            var allVariants = productVariantRepository.FindAll(x => x.ProductId == variant.ProductId).ToList();
            var allProductAttributes = productAttributeRepository.FindAll(x => allVariants.Select(v => v.Id).Contains(x.VariantId)).ToList();

            var attributeValues = attributeValueRepository.FindAll().ToDictionary(av => av.Id, av => av);
            var attributes = attributeRepository.FindAll().ToDictionary(a => a.Id, a => a);

            var selectedStorage = currentAttributes
                .Select(x => attributeValues[x.AvId])
                .FirstOrDefault(av => attributes[av.AttributeId].Name == "Dung lượng");

            if (selectedStorage is null) AppleException.ThrowNotFound(message: "Không xác định được dung lượng đã chọn");

            // Tìm các variant có cùng dung lượng
            var matchingVariantIds = allProductAttributes
                .Where(pa => attributeValues[pa.AvId].Value == selectedStorage.Value &&
                             attributes[attributeValues[pa.AvId].AttributeId].Name == "Dung lượng")
                .Select(pa => pa.VariantId)
                .Distinct()
                .ToList();

            // Tìm các màu sắc thuộc các variant đó
            var colorAttributes = new List<ProductAttributeFullDTO>();

            foreach (var pa in allProductAttributes
                        .Where(pa => matchingVariantIds.Contains(pa.VariantId) &&
                                     attributes[attributeValues[pa.AvId].AttributeId].Name == "Màu sắc"))
            {
                var av = attributeValues[pa.AvId];
                var thisVariant = await productVariantRepository.FindByIdAsync(pa.VariantId);
                if (thisVariant == null) continue;
                var image = await productImageRepository.FindSingleAsync(x => x.VariantId == thisVariant.Id && x.Position == 0, false);
                decimal price = GetDiscountedPrice(thisVariant, promotionRepository, productPromotionRepository);

                colorAttributes.Add(new ProductAttributeFullDTO
                {
                    AvId = av.Id,
                    AttributeId = av.AttributeId,
                    AttributeName = "Màu sắc",
                    AttributeValue = av.Value,
                    ImageUrl = image is not null ? fileService.GetFullPathFileServer(image.Url) : null,
                    VariantId = thisVariant.Id,
                    Price = price > 0 ? price : thisVariant.Price,
                });
            }

            colorAttributes = colorAttributes.GroupBy(a => a.AvId).Select(g => g.First()).ToList();
            var result = Result<List<ProductAttributeFullDTO>>.Ok(colorAttributes);

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

            return result;
        }

        private decimal GetDiscountedPrice(Entities.ProductVariant productVariant, IPromotionRepository promotionRepository, IProductPromotionRepository productPromotionRepository)
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