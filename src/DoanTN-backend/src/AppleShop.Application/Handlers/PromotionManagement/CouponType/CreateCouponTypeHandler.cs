using AppleShop.Application.Requests.PromotionManagement.CouponType;
using AppleShop.Application.Validators.PromotionManagement.CouponType;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.CouponType
{
    public class CreateCouponTypeHandler : IRequestHandler<CreateCouponTypeRequest, Result<object>>
    {
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IMapper mapper;

        public CreateCouponTypeHandler(ICouponTypeRepository couponTypeRepository, IMapper mapper)
        {
            this.couponTypeRepository = couponTypeRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(CreateCouponTypeRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateCouponTypeValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var couponType = mapper.Map<Entities.PromotionManagement.CouponType>(request);

            using var transaction = await couponTypeRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                couponType.Name = request.Name;
                couponTypeRepository.Create(couponType);
                await couponTypeRepository.SaveChangesAsync(cancellationToken);
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