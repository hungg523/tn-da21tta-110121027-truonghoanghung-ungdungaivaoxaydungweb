using AppleShop.Application.Requests.DTOs.StatisticalManagement.ProductReport;
using AppleShop.Application.Requests.StatisticalManagement.ProductReport;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.WishListManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.StatisticalManagement.ProductReport
{
    public class GetTopProductReportHandler : IRequestHandler<GetTopProductReportRequest, Result<List<TopProductReportDTO>>>
    {
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IProductViewRepository productViewRepository;
        private readonly IWishListRepository wishListRepository;
        private readonly IReviewRepository reviewRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IFileService fileService;
        private readonly ICartItemRepository cartItemRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly ICacheService cacheService;

        public GetTopProductReportHandler(
            IProductVariantRepository productVariantRepository,
            IProductRepository productRepository,
            IProductImageRepository productImageRepository,
            IProductAttributeRepository productAttributeRepository,
            IAttributeValueRepository attributeValueRepository,
            IOrderItemRepository orderItemRepository,
            IProductViewRepository productViewRepository,
            IWishListRepository wishListRepository,
            IReviewRepository reviewRepository,
            IPromotionRepository promotionRepository,
            IProductPromotionRepository productPromotionRepository,
            IFileService fileService,
            ICartItemRepository cartItemRepository,
            IAttributeRepository attributeRepository,
            ICacheService cacheService)
        {
            this.productVariantRepository = productVariantRepository;
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.orderItemRepository = orderItemRepository;
            this.productViewRepository = productViewRepository;
            this.wishListRepository = wishListRepository;
            this.reviewRepository = reviewRepository;
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.fileService = fileService;
            this.cartItemRepository = cartItemRepository;
            this.attributeRepository = attributeRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<TopProductReportDTO>>> Handle(GetTopProductReportRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"top_product_report_{request.StartDate}_{request.EndDate}_{request.Type}_{request.Limit}";
            var cachedResult = await cacheService.GetAsync<Result<List<TopProductReportDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var query = productVariantRepository.FindAll(x => x.IsActived == 1);
            var variants = await query.ToListAsync(cancellationToken);
            var variantIds = variants.Select(x => x.Id).ToList();

            var products = await productRepository.FindAll(x => variants.Select(v => v.ProductId).Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x, cancellationToken);
            var productImages = await productImageRepository.FindAll(x => variantIds.Contains(x.VariantId) && x.Position == 0).GroupBy(x => x.VariantId).ToDictionaryAsync(g => g.Key, g => g.First(), cancellationToken);

            var productAttributes = await productAttributeRepository.FindAll(x => variantIds.Contains(x.VariantId)).ToListAsync(cancellationToken);

            var attributes = await attributeRepository.FindAll().ToListAsync(cancellationToken);
            var attributeDict = attributes.ToDictionary(a => a.Id, a => a.Name);

            var attributeValues = await attributeValueRepository.FindAll().ToListAsync(cancellationToken);
            var attributeValueDict = attributeValues.ToDictionary(av => av.Id, av => av.Value);

            var promotions = await promotionRepository.FindAll(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActived == 1).ToListAsync(cancellationToken);
            var productPromotions = await productPromotionRepository.FindAll().ToListAsync(cancellationToken);

            var reviews = await reviewRepository.FindAll(x => variantIds.Contains(x.VariantId) && x.IsDeleted == false).GroupBy(r => r.VariantId).ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

            var result = new List<TopProductReportDTO>();

            switch (request.Type?.ToLower())
            {
                case "best-selling":
                    result = await GetBestSellingProducts(request, variants, products, productImages, productAttributes, attributeValues, promotions, productPromotions, reviews, attributes, attributeDict, attributeValueDict);
                    break;
                case "most-viewed":
                    result = await GetMostViewedProducts(request, variants, products, productImages, productAttributes, attributeValues, promotions, productPromotions, reviews, attributes, attributeDict, attributeValueDict);
                    break;
                case "most-carted":
                    result = await GetMostCartedProducts(request, variants, products, productImages, productAttributes, attributeValues, promotions, productPromotions, reviews, attributes, attributeDict, attributeValueDict);
                    break;
                case "most-wished":
                    result = await GetMostWishedProducts(request, variants, products, productImages, productAttributes, attributeValues, promotions, productPromotions, reviews, attributes, attributeDict, attributeValueDict);
                    break;
                case "highest-rated":
                    result = await GetHighestRatedProducts(request, variants, products, productImages, productAttributes, attributeValues, promotions, productPromotions, reviews, attributes, attributeDict, attributeValueDict);
                    break;
                default:
                    return Result<List<TopProductReportDTO>>.Errors("Invalid type.");
            }

            var finalResult = Result<List<TopProductReportDTO>>.Ok(result);
            await cacheService.SetAsync(cacheKey, finalResult, TimeSpan.FromMinutes(5));

            return finalResult;
        }

        private async Task<List<TopProductReportDTO>> GetBestSellingProducts(
            GetTopProductReportRequest request,
            List<Entities.ProductManagement.ProductVariant> variants,
            Dictionary<int?, Entities.ProductManagement.Product> products,
            Dictionary<int?, Entities.ProductManagement.ProductImage> productImages,
            List<Entities.ProductManagement.ProductAttribute> productAttributes,
            List<Entities.ProductManagement.AttributeValue> attributeValues,
            List<Entities.PromotionManagement.Promotion> promotions,
            List<Entities.PromotionManagement.ProductPromotion> productPromotions,
            Dictionary<int?, List<Entities.ReviewManagement.Review>> reviews,
            List<Entities.ProductManagement.Attribute> attributes,
            Dictionary<int?, string> attributeDict,
            Dictionary<int?, string> attributeValueDict)
        {
            var orderItems = await orderItemRepository.FindAll(x => x.ItemStatus == (int)ItemStatus.Delivered)
                .GroupBy(x => x.VariantId)
                .Select(g => new { VariantId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(request.Limit ?? 10)
                .ToListAsync();

            return orderItems.Select(item =>
            {
                var variant = variants.FirstOrDefault(v => v.Id == item.VariantId);
                if (variant == null) return null;

                var product = products.TryGetValue(variant.ProductId ?? 0, out var p) ? p : null;
                var image = productImages.TryGetValue(variant.Id ?? 0, out var img) ? img : null;
                var variantAttributes = productAttributes.Where(pa => pa.VariantId == variant.Id).ToList();
                var productReviews = reviews.TryGetValue(variant.Id ?? 0, out var r) ? r : new List<Entities.ReviewManagement.Review>();

                var fullAttributes = (from pa in variantAttributes
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
                    }).Select(a => a.AttributeValue).ToList();

                return new TopProductReportDTO
                {
                    VariantId = variant.Id,
                    Name = $"{product?.Name} {string.Join(" ", orderedAttributes)}",
                    Image = image != null ? fileService.GetFullPathFileServer(image.Url) : null,
                    Price = variant.Price,
                    DiscountPrice = GetDiscountedPrice(variant, promotions, productPromotions),
                    TotalQuantity = item.TotalQuantity,
                    AverageRating = productReviews.Any() ? productReviews.Average(r => r.Rating) : 0,
                    TotalReviews = productReviews.Count,
                    ProductAttributes = orderedAttributes
                };
            }).Where(x => x != null).ToList();
        }

        private async Task<List<TopProductReportDTO>> GetMostViewedProducts(
            GetTopProductReportRequest request,
            List<Entities.ProductManagement.ProductVariant> variants,
            Dictionary<int?, Entities.ProductManagement.Product> products,
            Dictionary<int?, Entities.ProductManagement.ProductImage> productImages,
            List<Entities.ProductManagement.ProductAttribute> productAttributes,
            List<Entities.ProductManagement.AttributeValue> attributeValues,
            List<Entities.PromotionManagement.Promotion> promotions,
            List<Entities.PromotionManagement.ProductPromotion> productPromotions,
            Dictionary<int?, List<Entities.ReviewManagement.Review>> reviews,
            List<Entities.ProductManagement.Attribute> attributes,
            Dictionary<int?, string> attributeDict,
            Dictionary<int?, string> attributeValueDict)
        {
            var productViews = await productViewRepository.FindAll()
                .GroupBy(x => x.VariantId)
                .Select(g => new { VariantId = g.Key, TotalViews = g.Sum(x => x.View) })
                .OrderByDescending(x => x.TotalViews)
                .Take(request.Limit ?? 10)
                .ToListAsync();

            return productViews.Select(view =>
            {
                var variant = variants.FirstOrDefault(v => v.Id == view.VariantId);
                if (variant == null) return null;

                var product = products.TryGetValue(variant.ProductId ?? 0, out var p) ? p : null;
                var image = productImages.TryGetValue(variant.Id ?? 0, out var img) ? img : null;
                var variantAttributes = productAttributes.Where(pa => pa.VariantId == variant.Id).ToList();
                var productReviews = reviews.TryGetValue(variant.Id ?? 0, out var r) ? r : new List<Entities.ReviewManagement.Review>();

                var fullAttributes = (from pa in variantAttributes
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
                    })
                    .Select(a => a.AttributeValue)
                    .ToList();

                return new TopProductReportDTO
                {
                    VariantId = variant.Id,
                    Name = $"{product?.Name} {string.Join(" ", orderedAttributes)}",
                    Image = image != null ? fileService.GetFullPathFileServer(image.Url) : null,
                    Price = variant.Price,
                    DiscountPrice = GetDiscountedPrice(variant, promotions, productPromotions),
                    TotalQuantity = view.TotalViews,
                    AverageRating = productReviews.Any() ? productReviews.Average(r => r.Rating) : 0,
                    TotalReviews = productReviews.Count,
                    ProductAttributes = orderedAttributes
                };
            }).Where(x => x != null).ToList();
        }

        private async Task<List<TopProductReportDTO>> GetMostCartedProducts(
            GetTopProductReportRequest request,
            List<Entities.ProductManagement.ProductVariant> variants,
            Dictionary<int?, Entities.ProductManagement.Product> products,
            Dictionary<int?, Entities.ProductManagement.ProductImage> productImages,
            List<Entities.ProductManagement.ProductAttribute> productAttributes,
            List<Entities.ProductManagement.AttributeValue> attributeValues,
            List<Entities.PromotionManagement.Promotion> promotions,
            List<Entities.PromotionManagement.ProductPromotion> productPromotions,
            Dictionary<int?, List<Entities.ReviewManagement.Review>> reviews,
            List<Entities.ProductManagement.Attribute> attributes,
            Dictionary<int?, string> attributeDict,
            Dictionary<int?, string> attributeValueDict)
        {
            var cartItems = await cartItemRepository.FindAll()
                .GroupBy(x => x.VariantId)
                .Select(g => new { VariantId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(request.Limit ?? 10)
                .ToListAsync();

            return cartItems.Select(item =>
            {
                var variant = variants.FirstOrDefault(v => v.Id == item.VariantId);
                if (variant == null) return null;

                var product = products.TryGetValue(variant.ProductId ?? 0, out var p) ? p : null;
                var image = productImages.TryGetValue(variant.Id ?? 0, out var img) ? img : null;
                var variantAttributes = productAttributes.Where(pa => pa.VariantId == variant.Id).ToList();
                var productReviews = reviews.TryGetValue(variant.Id ?? 0, out var r) ? r : new List<Entities.ReviewManagement.Review>();

                var fullAttributes = (from pa in variantAttributes
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
                    })
                    .Select(a => a.AttributeValue)
                    .ToList();

                return new TopProductReportDTO
                {
                    VariantId = variant.Id,
                    Name = $"{product?.Name} {string.Join(" ", orderedAttributes)}",
                    Image = image != null ? fileService.GetFullPathFileServer(image.Url) : null,
                    Price = variant.Price,
                    DiscountPrice = GetDiscountedPrice(variant, promotions, productPromotions),
                    TotalQuantity = item.TotalQuantity,
                    AverageRating = productReviews.Any() ? productReviews.Average(r => r.Rating) : 0,
                    TotalReviews = productReviews.Count,
                    ProductAttributes = orderedAttributes
                };
            }).Where(x => x != null).ToList();
        }

        private async Task<List<TopProductReportDTO>> GetMostWishedProducts(
            GetTopProductReportRequest request,
            List<Entities.ProductManagement.ProductVariant> variants,
            Dictionary<int?, Entities.ProductManagement.Product> products,
            Dictionary<int?, Entities.ProductManagement.ProductImage> productImages,
            List<Entities.ProductManagement.ProductAttribute> productAttributes,
            List<Entities.ProductManagement.AttributeValue> attributeValues,
            List<Entities.PromotionManagement.Promotion> promotions,
            List<Entities.PromotionManagement.ProductPromotion> productPromotions,
            Dictionary<int?, List<Entities.ReviewManagement.Review>> reviews,
            List<Entities.ProductManagement.Attribute> attributes,
            Dictionary<int?, string> attributeDict,
            Dictionary<int?, string> attributeValueDict)
        {
            var wishListItems = await wishListRepository.FindAll(x => x.IsActived == true)
                .GroupBy(x => x.VariantId)
                .Select(g => new { VariantId = g.Key, TotalWishes = g.Count() })
                .OrderByDescending(x => x.TotalWishes)
                .Take(request.Limit ?? 10)
                .ToListAsync();

            return wishListItems.Select(item =>
            {
                var variant = variants.FirstOrDefault(v => v.Id == item.VariantId);
                if (variant == null) return null;

                var product = products.TryGetValue(variant.ProductId ?? 0, out var p) ? p : null;
                var image = productImages.TryGetValue(variant.Id ?? 0, out var img) ? img : null;
                var variantAttributes = productAttributes.Where(pa => pa.VariantId == variant.Id).ToList();
                var productReviews = reviews.TryGetValue(variant.Id ?? 0, out var r) ? r : new List<Entities.ReviewManagement.Review>();

                var fullAttributes = (from pa in variantAttributes
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
                    })
                    .Select(a => a.AttributeValue)
                    .ToList();

                return new TopProductReportDTO
                {
                    VariantId = variant.Id,
                    Name = $"{product?.Name} {string.Join(" ", orderedAttributes)}",
                    Image = image != null ? fileService.GetFullPathFileServer(image.Url) : null,
                    Price = variant.Price,
                    DiscountPrice = GetDiscountedPrice(variant, promotions, productPromotions),
                    TotalQuantity = item.TotalWishes,
                    AverageRating = productReviews.Any() ? productReviews.Average(r => r.Rating) : 0,
                    TotalReviews = productReviews.Count,
                    ProductAttributes = orderedAttributes
                };
            }).Where(x => x != null).ToList();
        }

        private async Task<List<TopProductReportDTO>> GetHighestRatedProducts(
            GetTopProductReportRequest request,
            List<Entities.ProductManagement.ProductVariant> variants,
            Dictionary<int?, Entities.ProductManagement.Product> products,
            Dictionary<int?, Entities.ProductManagement.ProductImage> productImages,
            List<Entities.ProductManagement.ProductAttribute> productAttributes,
            List<Entities.ProductManagement.AttributeValue> attributeValues,
            List<Entities.PromotionManagement.Promotion> promotions,
            List<Entities.PromotionManagement.ProductPromotion> productPromotions,
            Dictionary<int?, List<Entities.ReviewManagement.Review>> reviews,
            List<Entities.ProductManagement.Attribute> attributes,
            Dictionary<int?, string> attributeDict,
            Dictionary<int?, string> attributeValueDict)
        {
            var ratedProducts = reviews
                .Select(kvp => new
                {
                    VariantId = kvp.Key,
                    AverageRating = kvp.Value.Average(r => r.Rating),
                    TotalReviews = kvp.Value.Count
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.TotalReviews)
                .Take(request.Limit ?? 10)
                .ToList();

            return ratedProducts.Select(item =>
            {
                var variant = variants.FirstOrDefault(v => v.Id == item.VariantId);
                if (variant == null) return null;

                var product = products.TryGetValue(variant.ProductId ?? 0, out var p) ? p : null;
                var image = productImages.TryGetValue(variant.Id ?? 0, out var img) ? img : null;
                var variantAttributes = productAttributes.Where(pa => pa.VariantId == variant.Id).ToList();
                var productReviews = reviews.TryGetValue(variant.Id ?? 0, out var r) ? r : new List<Entities.ReviewManagement.Review>();

                var fullAttributes = (from pa in variantAttributes
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
                    })
                    .Select(a => a.AttributeValue)
                    .ToList();

                return new TopProductReportDTO
                {
                    VariantId = variant.Id,
                    Name = $"{product?.Name} {string.Join(" ", orderedAttributes)}",
                    Image = image != null ? fileService.GetFullPathFileServer(image.Url) : null,
                    Price = variant.Price,
                    DiscountPrice = GetDiscountedPrice(variant, promotions, productPromotions),
                    TotalQuantity = item.TotalReviews,
                    AverageRating = item.AverageRating,
                    TotalReviews = item.TotalReviews,
                    ProductAttributes = orderedAttributes
                };
            }).Where(x => x != null).ToList();
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