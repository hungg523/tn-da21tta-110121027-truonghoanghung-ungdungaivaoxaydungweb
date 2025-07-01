using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductVariant
{
    public class SearchProductHandler : IRequestHandler<SearchProductRequest, Result<ProductVariantResponseDTO>>
    {
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductDetailRepository productDetailRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IFileService fileService;
        private readonly IReviewRepository reviewRepository;
        private readonly IUserRepository userRepository;

        public SearchProductHandler(IProductRepository productRepository,
                                        IProductImageRepository productImageRepository,
                                        IAttributeValueRepository attributeValueRepository,
                                        IProductAttributeRepository productAttributeRepository,
                                        IProductVariantRepository productVariantRepository,
                                        IPromotionRepository promotionRepository,
                                        IProductPromotionRepository productPromotionRepository,
                                        IProductDetailRepository productDetailRepository,
                                        ICategoryRepository categoryRepository,
                                        IFileService fileService,
                                        IReviewRepository reviewRepository,
                                        IUserRepository userRepository)
        {
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.productVariantRepository = productVariantRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.productDetailRepository = productDetailRepository;
            this.categoryRepository = categoryRepository;
            this.fileService = fileService;
            this.reviewRepository = reviewRepository;
            this.userRepository = userRepository;
        }

        public async Task<Result<ProductVariantResponseDTO>> Handle(SearchProductRequest request, CancellationToken cancellationToken)
        {
            var query = productVariantRepository.FindAll().ToList();
            var user = await userRepository.FindByIdAsync(request.UserId, false);
            if (user is null || user.Role == 0) query = query.Where(x => x.IsActived == 1).ToList();

            var productVariants = query;
            var productIds = productVariants.Select(v => v.ProductId).Distinct().ToList();

            var products = productRepository.FindAll(x => productIds.Contains(x.Id)).ToList();
            var productDict = products.ToDictionary(p => p.Id, p => p);

            var productImages = productImageRepository.FindAll(x => productVariants.Select(v => v.Id).Contains(x.VariantId)).ToList();
            var imagesDict = productImages.GroupBy(img => img.VariantId).ToDictionary(g => g.Key, g => g.ToList());

            var productAttributes = productAttributeRepository.FindAll(x => productVariants.Select(v => v.Id).Contains(x.VariantId)).ToList();

            var attributeValues = attributeValueRepository.FindAll().ToList();
            var attributeValueDict = attributeValues.ToDictionary(av => av.Id, av => av.Value);

            var promotions = promotionRepository.FindAll(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActived == 1).ToList();

            var productPromotions = productPromotionRepository.FindAll().ToList();
            var productPromotionDict = productPromotions.Where(pp => pp.ProductId.HasValue).GroupBy(pp => pp.ProductId.Value).ToDictionary(g => g.Key, g => g.ToList());

            var variantPromotionDict = productPromotions.Where(pp => pp.VariantId.HasValue).GroupBy(pp => pp.VariantId.Value).ToDictionary(g => g.Key, g => g.ToList());

            var productDetails = productDetailRepository.FindAll().ToList();
            var productDetailDict = productDetails.GroupBy(pd => pd.ProductId).ToDictionary(g => g.Key, g => g.Select(pd => new DetailDTO { Key = pd.DetailKey, Value = pd.DetailValue }).ToList());

            var reviews = reviewRepository.FindAll(x => productVariants.Select(x => x.Id).Contains(x.VariantId) && x.IsDeleted == false).GroupBy(r => r.VariantId).ToDictionary(g => g.Key, g => g.ToList());

            var productDtos = productVariants.Select(variant =>
            {
                var baseName = productDict.TryGetValue(variant.ProductId, out var product) ? product.Name : "Unknown";
                var attributeList = productAttributes.Where(pa => pa.VariantId == variant.Id)
                    .Select(pa => new ProductAttributeFullDTO
                    {
                        AvId = pa.AvId,
                        AttributeValue = attributeValueDict.TryGetValue(pa.AvId, out var value) ? value : "Unknown"
                    }).ToList();

                var variantName = baseName + " " + string.Join(" ", attributeList.Select(a => a.AttributeValue));

                var discountPercentage = 0;
                var discountAmount = 0;

                if (variantPromotionDict.TryGetValue(variant.Id.Value, out var variantPromotions))
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
                else if (productPromotionDict.TryGetValue(variant.ProductId.Value, out var productPromotions))
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

                var finalDiscount = discountPercentage > 0 ? variant.Price * discountPercentage / 100 : discountAmount;
                var discountedPrice = Math.Max((decimal)(variant.Price - finalDiscount), 0);

                var productReviews = reviews.TryGetValue(variant.Id, out var variantReviews) ? variantReviews.ToList() : new List<Entities.ReviewManagement.Review>();
                var totalReviews = productReviews.Count;
                var totalStars = productReviews.Sum(r => r.Rating);
                var averageRating = totalReviews > 0 ? totalStars / (double)totalReviews : 0;

                var specifications = productDetailDict.TryGetValue(variant.ProductId, out var details) ? details.ToList() : new List<DetailDTO>();
                var storageAttribute = attributeList.FirstOrDefault(attr => attr.AttributeValue?.ToLower().Contains("gb") == true);
                if (storageAttribute != null)
                {
                    var storageSpec = specifications.FirstOrDefault(s => s.Key == "Bộ nhớ trong");
                    if (storageSpec != null)
                    {
                        storageSpec.Value = storageAttribute.AttributeValue;
                    }
                    else
                    {
                        specifications.Add(new DetailDTO { Key = "Bộ nhớ trong", Value = storageAttribute.AttributeValue });
                    }
                }

                return new ProductFullDTO
                {
                    VariantId = variant.Id,
                    //ProductId = variant.ProductId,
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
                        }).ToList()
                        : new List<ProductImageDTO>(),

                    Details = specifications
                };
            });

            if (!string.IsNullOrWhiteSpace(request.Name)) productDtos = productDtos.Where(x => x.Name.ToLower().Contains(request.Name.ToLower()));

            var totalItems = productDtos.Count();
            var pagedDtos = productDtos.Skip(request.Skip ?? 0).Take(request.Take ?? totalItems).ToList();

            return Result<ProductVariantResponseDTO>.Ok(new ProductVariantResponseDTO
            {
                TotalItems = totalItems,
                ProductVariants = pagedDtos
            });
        }
    }
}