using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using AppleShop.Application.Validators.ProductManagement.ProductVariant;
using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.AIManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductVariant
{
    public class GetDetailProductVariantHandler : IRequestHandler<GetDetailProductVariantRequest, Result<ProductFullDTO>>
    {
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductDetailRepository productDetailRepository;
        private readonly IFileService fileService;
        private readonly IReviewRepository reviewRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly IUserRepository userRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IUserInteractionRepository userInteractionRepository;
        private readonly IProductViewRepository productViewRepository;
        private readonly ICacheService cacheService;

        public GetDetailProductVariantHandler(IProductRepository productRepository,
                                        IProductImageRepository productImageRepository,
                                        IAttributeValueRepository attributeValueRepository,
                                        IProductAttributeRepository productAttributeRepository,
                                        IProductVariantRepository productVariantRepository,
                                        IPromotionRepository promotionRepository,
                                        IProductPromotionRepository productPromotionRepository,
                                        IProductDetailRepository productDetailRepository,
                                        IFileService fileService,
                                        IReviewRepository reviewRepository,
                                        IAttributeRepository attributeRepository,
                                        IUserRepository userRepository,
                                        ICategoryRepository categoryRepository,
                                        IUserInteractionRepository userInteractionRepository,
                                        IProductViewRepository productViewRepository,
                                        ICacheService cacheService)
        {
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.productVariantRepository = productVariantRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.productDetailRepository = productDetailRepository;
            this.fileService = fileService;
            this.reviewRepository = reviewRepository;
            this.attributeRepository = attributeRepository;
            this.userRepository = userRepository;
            this.categoryRepository = categoryRepository;
            this.userInteractionRepository = userInteractionRepository;
            this.productViewRepository = productViewRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<ProductFullDTO>> Handle(GetDetailProductVariantRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"product_detail_{request.VariantId}";
            var cachedResult = await cacheService.GetAsync<Result<ProductFullDTO>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var validator = new GetDetailProductVariantValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var variant = await productVariantRepository.FindByIdAsync(request.VariantId);
            if (variant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

            var product = await productRepository.FindByIdAsync(variant.ProductId);
            if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product));

            var allVariants = productVariantRepository.FindAll(x => x.ProductId == product.Id).ToList();

            var productImages = productImageRepository.FindAll(x => x.VariantId == variant.Id).ToList();

            var productAttributes = productAttributeRepository.FindAll(x => x.VariantId == variant.Id).ToList();
            var allProductAttributes = productAttributeRepository.FindAll(x => allVariants.Select(x => x.Id).Contains(x.VariantId)).ToList();
            var attributeValues = attributeValueRepository.FindAll().ToDictionary(av => av.Id, av => av);

            var attributes = attributeRepository.FindAll(x => attributeValues.Select(x => x.Value.AttributeId).Contains(x.Id)).ToDictionary(a => a.Id, a => a);

            decimal originalPrice = GetDiscountedPrice(variant, promotionRepository, productPromotionRepository);
            
            var fullAttributes = (from pa in productAttributes
                                  where pa.VariantId == variant.Id
                                  join av in attributeValues.Values on pa.AvId equals av.Id
                                  join attr in attributes.Values on av.AttributeId equals attr.Id
                                  select new
                                  {
                                      AttributeName = attr.Name,
                                      AttributeValue = av.Value
                                  }).ToList();

            string[] desiredOrder = { "kích thước", "màu sắc", "ram", "dung lượng", "cổng sạc" };

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

            var baseName = product.Name;
            var variantName = baseName + " " + string.Join(" ", orderedAttributes.Select(a => a.AttributeValue));

            var attribute = productAttributes.Select(pa =>
            {
                attributeValues.TryGetValue(pa.AvId, out var attributeValue);
                attributes.TryGetValue(pa.AvId, out var attribute);

                return new ProductAttributeFullDTO
                {
                    AvId = pa.AvId,
                    AttributeValue = attributeValue?.Value ?? "Unknown",
                };
            }).ToList();

            var attributeList = new List<ProductAttributeFullDTO>();
            var seenValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var storageAttributeIds = attributes.Where(a => a.Value.Name == "Dung lượng").Select(a => a.Key).ToHashSet();

            foreach (var apa in allProductAttributes)
            {
                if (!attributeValues.TryGetValue(apa.AvId, out var attributeValue)) continue;
                if (!storageAttributeIds.Contains(attributeValue.AttributeId)) continue;
                if (!seenValues.Add(attributeValue.Value)) continue;

                var productVariant = await productVariantRepository.FindByIdAsync(apa.VariantId);
                if (productVariant is null) continue;

                decimal price = GetDiscountedPrice(productVariant, promotionRepository, productPromotionRepository);

                attributeList.Add(new ProductAttributeFullDTO
                {
                    AvId = apa.AvId,
                    VariantId = apa.VariantId,
                    AttributeId = attributeValue?.Id,
                    AttributeName = "Dung lượng",
                    AttributeValue = attributeValue?.Value ?? "Unknown",
                    Price = price > 0 ? price : productVariant.Price,
                });
            }

            var reviews = reviewRepository.FindAll(x => variant.Id == x.VariantId && x.IsDeleted == false).GroupBy(r => r.VariantId).ToDictionary(g => g.Key, g => g.ToList());
            var productReviews = reviews.TryGetValue(variant.Id, out var variantReviews) ? variantReviews.ToList() : new List<Entities.ReviewManagement.Review>();
            var totalReviews = productReviews.Count;
            var totalStars = productReviews.Sum(r => r.Rating);
            var averageRating = totalReviews > 0 ? totalStars / (double)totalReviews : 0;

            var specifications = productDetailRepository.FindAll(x => x.ProductId == product.Id)
                .Select(pd => new DetailDTO { Key = pd.DetailKey, Value = pd.DetailValue }).ToList();

            var storageAttribute = attribute.FirstOrDefault(attr => attr.AttributeValue?.ToLower().Contains("gb") == true);
            if (storageAttribute != null)
            {
                var storageSpec = specifications.FirstOrDefault(s => s.Key == "Bộ nhớ trong");
                if (storageSpec != null) storageSpec.Value = storageAttribute.AttributeValue;
                else specifications.Add(new DetailDTO { Key = "Bộ nhớ trong", Value = storageAttribute.AttributeValue });
            }

            var productDto = new ProductFullDTO
            {
                VariantId = variant.Id,
                ProductId = variant.ProductId,
                Name = variantName.Trim(),
                Description = product.Description,
                Price = variant.Price,
                DiscountPrice = originalPrice,
                IsActived = variant.IsActived,
                Stock = variant.Stock,
                ReservedStock = Math.Min((int)variant.ReservedStock, (int)variant.Stock),
                ActualStock = Math.Max(0, (int)variant.Stock - (int)variant.ReservedStock),
                SoldQuantity = variant.SoldQuantity,
                Images = productImages.Select(image => new ProductImageDTO
                {
                    Id = image.Id,
                    Title = image.Title,
                    Url = fileService.GetFullPathFileServer(image.Url),
                    Position = image.Position,
                }).OrderBy(x => x.Position).ToList(),

                ProductsAttributes = attributeList.ToList(),
                Details = specifications
            };

            var user = await userRepository.FindByIdAsync(request.UserId, false);
            if (user is not null)
            {
                var category = await categoryRepository.FindByIdAsync(product.CategoryId, false);
                var existingProductView = await productViewRepository.FindSingleAsync(x => x.UserId == user.Id && x.VariantId == variant.Id && x.ProductId == product.Id, true);
                var als = new UserInteraction
                {
                    UserId = user.Id,
                    VariantId = variant.Id,
                    Type = "click",
                    Value = 2,
                    CreatedAt = DateTime.Now,
                };
                userInteractionRepository.Create(als);
                
                if (existingProductView is not null)
                {
                    existingProductView.View += 1;
                    existingProductView.UpdatedAt = DateTime.Now;
                    productViewRepository.Update(existingProductView);
                }
                else
                {
                    var productView = new ProductView
                    {
                        VariantId = variant.Id,
                        ProductId = product.Id,
                        CategoryId = category.Id,
                        UserId = user.Id,
                        View = 1,
                        CreatedAt = DateTime.Now,
                    };
                    productViewRepository.Create(productView);
                }

                await productViewRepository.SaveChangesAsync(cancellationToken);
                await userInteractionRepository.SaveChangesAsync(cancellationToken);
            }

            var result = Result<ProductFullDTO>.Ok(productDto);

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

            return result;
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