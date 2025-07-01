using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using AppleShop.Application.Validators.ProductManagement.ProductVariant;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductVariant
{
    public class GetAllProductVariantHandler : IRequestHandler<GetAllProductVariantRequest, Result<ProductVariantResponseDTO>>
    {
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IFileService fileService;
        private readonly IReviewRepository reviewRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly ICacheService cacheService;

        public GetAllProductVariantHandler(IProductRepository productRepository,
                                        IProductImageRepository productImageRepository,
                                        IAttributeValueRepository attributeValueRepository,
                                        IProductAttributeRepository productAttributeRepository,
                                        IProductVariantRepository productVariantRepository,
                                        IPromotionRepository promotionRepository,
                                        IProductPromotionRepository productPromotionRepository,
                                        ICategoryRepository categoryRepository,
                                        IFileService fileService,
                                        IReviewRepository reviewRepository,
                                        IAttributeRepository attributeRepository,
                                        ICacheService cacheService)
        {
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.productVariantRepository = productVariantRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.categoryRepository = categoryRepository;
            this.fileService = fileService;
            this.reviewRepository = reviewRepository;
            this.attributeRepository = attributeRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<ProductVariantResponseDTO>> Handle(GetAllProductVariantRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetAllProductVariantValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var cacheKey = $"product_variants_{request.IsActived}_{request.CategoryId}_{request.MinPrice}_{request.MaxPrice}_{request.Color}_{request.Storage}_{request.IsDescending}_{request.IsAscending}_{request.Skip}_{request.Take}";

            var cachedResult = await cacheService.GetAsync<Result<ProductVariantResponseDTO>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var query = productVariantRepository.FindAll().ToList();
            if (request.IsActived is not null && request.CategoryId is not null)
            {
                var categoryIds = GetAllChildCategoryIds(request.CategoryId.Value);
                var productsInCategory = productRepository.FindAll(x => categoryIds.Contains(x.CategoryId.Value) && x.IsActived == 1).ToList();
                var productIdsInCategory = productsInCategory.Select(p => p.Id).ToList();

                query = query.Where(x => productIdsInCategory.Contains(x.ProductId) && x.IsActived == 1).ToList();
            }

            if (request.IsActived is not null && request.CategoryId is null) 
                query = query.Where(x => x.IsActived == request.IsActived).ToList();

            var productVariants = query;
            var productIds = productVariants.Select(v => v.ProductId).Distinct().ToList();

            var products = productRepository.FindAll(x => productIds.Contains(x.Id)).ToList();
            var productDict = products.ToDictionary(p => p.Id, p => p);

            var productImages = productImageRepository.FindAll(x => productVariants.Select(v => v.Id).Contains(x.VariantId)).ToList();
            var imagesDict = productImages.GroupBy(img => img.VariantId).ToDictionary(g => g.Key, g => g.ToList());

            var productAttributes = productAttributeRepository.FindAll(x => productVariants.Select(v => v.Id).Contains(x.VariantId)).ToList();

            var attributes = attributeRepository.FindAll().ToList();
            var attributeDict = attributes.ToDictionary(a => a.Id, a => a.Name);

            var attributeValues = attributeValueRepository.FindAll().ToList();
            var attributeValueDict = attributeValues.ToDictionary(av => av.Id, av => av.Value);

            var reviews = reviewRepository.FindAll(x => productVariants.Select(x => x.Id).Contains(x.VariantId) && x.IsDeleted == false).GroupBy(r => r.VariantId).ToDictionary(g => g.Key, g => g.ToList());

            var productDtos = productVariants.Select(variant =>
            {
                var fullAttributes = (from pa in productAttributes
                                      where pa.VariantId == variant.Id
                                      join av in attributeValues on pa.AvId equals av.Id
                                      join attr in attributes on av.AttributeId equals attr.Id
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

                var baseName = productDict.TryGetValue(variant.ProductId, out var product) ? product.Name : "Unknown";
                var attributeList = productAttributes.Where(pa => pa.VariantId == variant.Id)
                    .Select(pa => new ProductAttributeFullDTO
                    {
                        AvId = pa.AvId,
                        AttributeValue = attributeValueDict.TryGetValue(pa.AvId, out var value) ? value : "Unknown"
                    }).ToList();

                var variantName = baseName + " " + string.Join(" ", orderedAttributes.Select(a => a.AttributeValue));
                var discountedPrice = GetDiscountedPrice(variant, promotionRepository, productPromotionRepository);

                var productReviews = reviews.TryGetValue(variant.Id, out var variantReviews) ? variantReviews.ToList() : new List<Entities.ReviewManagement.Review>();
                var totalReviews = productReviews.Count;
                var totalStars = productReviews.Sum(r => r.Rating);
                var averageRating = totalReviews > 0 ? totalStars / (double)totalReviews : 0;

                return new ProductFullDTO
                {
                    VariantId = variant.Id,
                    ProductId = variant.ProductId,
                    CategoryId = productDict[variant.ProductId].CategoryId,
                    Name = variantName.Trim(),
                    Description = product?.Description,
                    Price = variant.Price,
                    DiscountPrice = discountedPrice,
                    IsActived = variant.IsActived,
                    Stock = variant.Stock,
                    ReservedStock = Math.Min((int)variant.ReservedStock, (int)variant.Stock),
                    ActualStock = Math.Max(0, (int)variant.Stock - (int)variant.ReservedStock),
                    SoldQuantity = variant.SoldQuantity,
                    AverageRating = averageRating,
                    TotalReviews = totalReviews,
                    Images = imagesDict.TryGetValue(variant.Id, out var images)
                        ? images.Select(image => new ProductImageDTO
                        {
                            Id = image.Id,
                            Title = image.Title,
                            Url = fileService.GetFullPathFileServer(image.Url),
                            Position = image.Position,
                        }).OrderBy(x => x.Position).ToList()
                        : new List<ProductImageDTO>(),
                };
            });

            if (!string.IsNullOrWhiteSpace(request.Color))
            {
                var colors = request.Color
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim().ToLower())
                    .ToList();

                productDtos = productDtos.Where(p => colors.Any(color => p.Name.ToLower().Contains(color)));
            }

            if (!string.IsNullOrWhiteSpace(request.Storage))
            {
                var storages = request.Storage
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .ToList();

                productDtos = productDtos.Where(p => storages.Any(storage => p.Name.ToLower().Contains(storage)));
            }

            if (request.MinPrice is not null && request.MaxPrice is null) productDtos = productDtos.Where(x => x.DiscountPrice >= request.MinPrice);
            if (request.MinPrice is null && request.MaxPrice is not null) productDtos = productDtos.Where(x => x.DiscountPrice <= request.MaxPrice);
            if (request.MinPrice is not null && request.MaxPrice is not null) productDtos = productDtos.Where(x => x.DiscountPrice >= request.MinPrice && x.DiscountPrice <= request.MaxPrice);
            if (request.IsDescending == true) productDtos = productDtos.OrderByDescending(x => x.DiscountPrice);
            if (request.IsAscending == true) productDtos = productDtos.OrderBy(x => x.DiscountPrice);

            var totalItems = productDtos.Count();
            var result = Result<ProductVariantResponseDTO>.Ok(new ProductVariantResponseDTO
            {
                TotalItems = totalItems,
                ProductVariants = productDtos.Skip(request.Skip ?? 0).Take(request.Take ?? totalItems).ToList(),
            });

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

            return result;
        }

        public List<int> GetAllChildCategoryIds(int categoryId)
        {
            var categoryTree = new List<int> { categoryId };

            var childCategories = categoryRepository.FindAll(x => x.CatPid == categoryId).ToList();
            foreach (var child in childCategories)
            {
                categoryTree.AddRange(GetAllChildCategoryIds(child.Id.Value));
            }

            return categoryTree;
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