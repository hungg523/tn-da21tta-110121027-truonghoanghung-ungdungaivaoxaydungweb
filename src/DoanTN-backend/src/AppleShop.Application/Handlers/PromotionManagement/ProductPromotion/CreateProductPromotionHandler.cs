using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using AppleShop.Application.Validators.PromotionManagement.ProductPromotion;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.ProductPromotion
{
    public class CreateProductPromotionHandler : IRequestHandler<CreateProductPromotionRequest, Result<object>>
    {
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductRepository productRepository;
        private readonly IPromotionRepository promotionRepository;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;

        public CreateProductPromotionHandler(IProductPromotionRepository productPromotionRepository, IProductVariantRepository productVariantRepository, IProductRepository productRepository, IPromotionRepository promotionRepository, IMapper mapper, ICacheService cacheService)
        {
            this.productPromotionRepository = productPromotionRepository;
            this.productVariantRepository = productVariantRepository;
            this.productRepository = productRepository;
            this.promotionRepository = promotionRepository;
            this.mapper = mapper;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateProductPromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateProductPromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var promotion = await promotionRepository.FindByIdAsync(request.PromotionId, true);
            if (promotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Promotion));

            if (request.ProductIds is not null)
            {
                var products = await productRepository.FindByIds(request.ProductIds, false, cancellationToken);
                var invalidProductIds = request.ProductIds.Except(products.Select(d => d.Id.Value)).ToList();
                if (invalidProductIds.Any()) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product), string.Join("Product has not found by Id ", invalidProductIds));

                var duplicateProductIds = request.ProductIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateProductIds.Any()) AppleException.ThrowConflict(string.Join("Duplication Product Id found with Id ", duplicateProductIds));
            }

            if (request.VariantIds is not null)
            {
                var variants = await productVariantRepository.FindByIds(request.VariantIds, false, cancellationToken);
                var invalidVariantIds = request.VariantIds.Except(variants.Select(d => d.Id.Value)).ToList();
                if (invalidVariantIds.Any()) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant), string.Join("Product Variant has not found by Id ", invalidVariantIds));

                var duplicateVariantIds = request.VariantIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateVariantIds.Any()) AppleException.ThrowConflict(string.Join("Duplication Product Variant Id found with Id ", duplicateVariantIds));
            }

            using var transaction = await productPromotionRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.ProductIds is not null)
                {
                    var productPromotion = request.ProductIds.Select(productId => new Entities.PromotionManagement.ProductPromotion
                    {
                        ProductId = productId,
                        PromotionId = request.PromotionId,
                    }).ToList();
                    productPromotionRepository.AddRange(productPromotion);
                }

                if (request.VariantIds is not null)
                {
                    var productPromotion = request.VariantIds.Select(variantId => new Entities.PromotionManagement.ProductPromotion
                    {
                        VariantId = variantId,
                        PromotionId = request.PromotionId,
                    }).ToList();
                    productPromotionRepository.AddRange(productPromotion);
                }

                if (request.ProductIds is null && request.VariantIds is null)
                {
                    var productPromotion = new Entities.PromotionManagement.ProductPromotion { PromotionId = request.PromotionId };
                    productPromotionRepository.Create(productPromotion);
                }

                await productPromotionRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.RemoveByPatternAsync("product_variants_*");
                foreach (var variantId in request.VariantIds) await cacheService.RemoveAsync($"product_detail_{variantId}");
                if (promotion is not null && promotion.IsFlashSale == true) await cacheService.RemoveAsync("flash_sale_products");

                return Result<object>.Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}