using AppleShop.Application.Requests.PromotionManagement.Promotion;
using AppleShop.Application.Validators.PromotionManagement.Promotion;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Promotion
{
    public class GetDetailPromotionHandler : IRequestHandler<GetDetailPromotionRequest, Result<Entities.PromotionManagement.Promotion>>
    {
        private readonly IPromotionRepository promotionRepository;

        public GetDetailPromotionHandler(IPromotionRepository promotionRepository)
        {
            this.promotionRepository = promotionRepository;
        }

        public async Task<Result<Entities.PromotionManagement.Promotion>> Handle(GetDetailPromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailPromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var promotion = await promotionRepository.FindByIdAsync(request.Id);
            if (promotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Promotion));

            return Result<Entities.PromotionManagement.Promotion>.Ok(promotion);
        }
    }
}