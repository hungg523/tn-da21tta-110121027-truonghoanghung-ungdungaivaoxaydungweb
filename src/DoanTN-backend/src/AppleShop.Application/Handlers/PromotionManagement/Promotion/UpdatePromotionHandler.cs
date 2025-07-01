using AppleShop.Application.Requests.PromotionManagement.Promotion;
using AppleShop.Application.Validators.PromotionManagement.Promotion;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Promotion
{
    public class UpdatePromotionHandler : IRequestHandler<UpdatePromotionRequest, Result<object>>
    {
        private readonly IPromotionRepository promotionRepository;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;
        private readonly IProductPromotionRepository productPromotionRepository;

        public UpdatePromotionHandler(IPromotionRepository promotionRepository, IMapper mapper, ICacheService cacheService, IProductPromotionRepository productPromotionRepository)
        {
            this.promotionRepository = promotionRepository;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.productPromotionRepository = productPromotionRepository;
        }

        public async Task<Result<object>> Handle(UpdatePromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdatePromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var promotion = await promotionRepository.FindByIdAsync(request.Id, true);
            if (promotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Promotion));

            var productPromotion = productPromotionRepository.FindAll(x => x.PromotionId == promotion.Id).ToList();

            using var transaction = await promotionRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                promotion.IsFlashSale = request.IsFlashSale ?? promotion.IsFlashSale;
                mapper.Map(request, promotion);
                promotion.DiscountAmout = request.DiscountAmount;
                promotionRepository.Update(promotion);
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