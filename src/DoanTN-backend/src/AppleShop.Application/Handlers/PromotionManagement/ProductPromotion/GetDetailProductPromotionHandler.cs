using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using AppleShop.Application.Validators.PromotionManagement.ProductPromotion;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.ProductPromotion
{
    public class GetDetailProductPromotionHandler : IRequestHandler<GetDetailProductPromotionRequest, Result<Entities.PromotionManagement.ProductPromotion>>
    {
        private readonly IProductPromotionRepository productPromotionRepository;

        public GetDetailProductPromotionHandler(IProductPromotionRepository productPromotionRepository)
        {
            this.productPromotionRepository = productPromotionRepository;
        }

        public async Task<Result<Entities.PromotionManagement.ProductPromotion>> Handle(GetDetailProductPromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailProductPromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productPromotion = await productPromotionRepository.FindByIdAsync(request.Id, true);
            if (productPromotion is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.ProductPromotion));

            return Result<Entities.PromotionManagement.ProductPromotion>.Ok(productPromotion);
        }
    }
}