using AppleShop.Application.Validators.PromotionManagement.CouponType;
using Entities = AppleShop.Domain.Entities;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Application.Requests.PromotionManagement.CouponType;

namespace AppleShop.Application.Handlers.PromotionManagement.CouponType
{
    public class UpdateCouponTypeHandler : IRequestHandler<UpdateCouponTypeRequest, Result<object>>
    {
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IMapper mapper;

        public UpdateCouponTypeHandler(ICouponTypeRepository couponTypeRepository, IMapper mapper)
        {
            this.couponTypeRepository = couponTypeRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(UpdateCouponTypeRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateCouponTypeValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var couponType = await couponTypeRepository.FindByIdAsync(request.Id, true);
            if (couponType is not null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.CouponType));

            mapper.Map(request, couponType);

            using var transaction = await couponTypeRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                couponTypeRepository.Update(couponType);
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