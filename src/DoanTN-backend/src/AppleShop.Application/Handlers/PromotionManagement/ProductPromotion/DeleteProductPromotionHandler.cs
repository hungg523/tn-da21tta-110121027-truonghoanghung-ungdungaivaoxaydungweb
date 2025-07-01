using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using AppleShop.Application.Validators.PromotionManagement.ProductPromotion;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Infrastructure.Repositories.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.ProductPromotion
{
    public class DeleteProductPromotionHandler : IRequestHandler<DeleteProductPromotionRequest, Result<object>>
    {
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly ICacheService cacheService;
        private readonly IPromotionRepository promotionRepository;

        public DeleteProductPromotionHandler(IProductPromotionRepository productPromotionRepository, ICacheService cacheService, IPromotionRepository promotionRepository)
        {
            this.productPromotionRepository = productPromotionRepository;
            this.cacheService = cacheService;
            this.promotionRepository = promotionRepository;
        }

        public async Task<Result<object>> Handle(DeleteProductPromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteProductPromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productPromotion = await productPromotionRepository.FindByIdAsync(request.Id, true);
            if (productPromotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.ProductPromotion));

            var isFlashSale = await promotionRepository.FindSingleAsync(x => x.Id == productPromotion.PromotionId && x.IsFlashSale == true, false);

            using var transaction = await productPromotionRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                productPromotionRepository.Delete(productPromotion!);
                await productPromotionRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.RemoveByPatternAsync("product_variants_*");
                await cacheService.RemoveAsync($"product_detail_{productPromotion.VariantId}");
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