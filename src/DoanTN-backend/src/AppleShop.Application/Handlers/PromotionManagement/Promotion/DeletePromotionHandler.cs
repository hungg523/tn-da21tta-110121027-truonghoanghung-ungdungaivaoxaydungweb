using AppleShop.Application.Requests.PromotionManagement.Promotion;
using AppleShop.Application.Validators.PromotionManagement.Promotion;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Promotion
{
    public class DeletePromotionHandler : IRequestHandler<DeletePromotionRequest, Result<object>>
    {
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly ICacheService cacheService;

        public DeletePromotionHandler(IPromotionRepository promotionRepository, IProductPromotionRepository productPromotionRepository, ICacheService cacheService)
        {
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(DeletePromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeletePromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var promotion = await promotionRepository.FindSingleAsync(x => x.Id == request.Id && x.IsActived != 0, true);
            if (promotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Promotion));

            var productPromotion = productPromotionRepository.FindAll(x => x.PromotionId == promotion.Id).ToList();

            using var transaction = await promotionRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                promotion.IsActived = 0;
                promotionRepository.Update(promotion!);
                await promotionRepository.SaveChangesAsync(cancellationToken);

                transaction.Commit();
                if (productPromotion.Any())
                {
                    await cacheService.RemoveByPatternAsync("product_variants_*");
                    foreach (var item in productPromotion) await cacheService.RemoveAsync($"product_detail_{item.VariantId}");
                }

                if (promotion.IsFlashSale == true) await cacheService.RemoveAsync("flash_sale_products");
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