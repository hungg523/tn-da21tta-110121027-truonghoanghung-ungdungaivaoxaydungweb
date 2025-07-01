using AppleShop.Application.Requests.PromotionManagement.Promotion;
using AppleShop.Application.Validators.PromotionManagement.Promotion;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Promotion
{
    public class CreatePromotionHandler : IRequestHandler<CreatePromotionRequest, Result<object>>
    {
        private readonly IPromotionRepository promotionRepository;
        private readonly IMapper mapper;

        public CreatePromotionHandler(IPromotionRepository promotionRepository, IMapper mapper)
        {
            this.promotionRepository = promotionRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(CreatePromotionRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreatePromotionValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var promotion = mapper.Map<Entities.PromotionManagement.Promotion>(request);

            using var transaction = await promotionRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                promotion.DiscountAmout = request.DiscountAmount;
                promotion.IsFlashSale = request.IsFlashSale;
                promotionRepository.Create(promotion);
                await promotionRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();
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