using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using AppleShop.Application.Validators.PromotionManagement.ProductPromotion;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductManagement.PromotionManagement.ProductPromotion
{
    public class UpdateProductPromoitonHandler : IRequestHandler<UpdateProductPromotionRequest, Result<object>>
    {
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IPromotionRepository promotionRepository;
        private readonly ICacheService cacheService;

        public UpdateProductPromoitonHandler(IProductPromotionRepository productPromotionRepository, IProductVariantRepository productVariantRepository, IProductRepository productRepository, IMapper mapper, IPromotionRepository promotionRepository, ICacheService cacheService)
        {
            this.productPromotionRepository = productPromotionRepository;
            this.productVariantRepository = productVariantRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.promotionRepository = promotionRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(UpdateProductPromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateProductPromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productPromotion = await productPromotionRepository.FindByIdAsync(request.Id, true);
            if (productPromotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.ProductPromotion));

            var isFlashSale = await promotionRepository.FindSingleAsync(x => x.Id == productPromotion.PromotionId && x.IsFlashSale == true, false);

            var existingEntity = await productPromotionRepository.FindSingleAsync(x => x.VariantId == request.Id && x.PromotionId == request.PromotionId, true);
            if (existingEntity is not null) AppleException.ThrowConflict("Product Promotion has existed in database.");

            if (request.ProductId is not null)
            {
                var product = await productRepository.FindByIdAsync(request.ProductId!, true);
                if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product));
            }

            if (request.VariantId is not null)
            {
                var productVariant = await productVariantRepository.FindByIdAsync(request.VariantId!, true);
                if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.ProductPromotion));
            }

            if (request.PromotionId is not null)
            {
                var promotion = await promotionRepository.FindByIdAsync(request.PromotionId, true);
                if (promotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Promotion));
            }

            using var transaction = await productPromotionRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.ProductId is not null && request.VariantId is null)
                {
                    productPromotion.ProductId = request.ProductId;
                    productPromotion.PromotionId = request.PromotionId ?? productPromotion.PromotionId;
                    productPromotionRepository.Update(productPromotion!);
                }

                if (request.ProductId is null && request.VariantId is not null)
                {
                    productPromotion.VariantId = request.VariantId;
                    productPromotion.PromotionId = request.PromotionId ?? productPromotion.PromotionId;
                    productPromotionRepository.Update(productPromotion!);
                }

                if (request.ProductId is null && request.VariantId is null)
                {
                    mapper.Map(request, productPromotion);
                    productPromotionRepository.Update(productPromotion!);
                }

                await productPromotionRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.RemoveByPatternAsync("product_variants_*");
                if (isFlashSale is not null) await cacheService.RemoveAsync("flash_sale_products");

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