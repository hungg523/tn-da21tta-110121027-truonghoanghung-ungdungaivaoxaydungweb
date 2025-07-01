using AppleShop.Application.Requests.PromotionManagement.Coupon;
using AppleShop.Application.Validators.PromotionManagement.Coupon;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Coupon
{
    public class UpdateCouponHandler : IRequestHandler<UpdateCouponRequest, Result<object>>
    {
        private readonly ICouponRepository couponRepository;
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IMapper mapper;

        public UpdateCouponHandler(ICouponRepository couponRepository, IMapper mapper, ICouponTypeRepository couponTypeRepository)
        {
            this.couponRepository = couponRepository;
            this.mapper = mapper;
            this.couponTypeRepository = couponTypeRepository;
        }

        public async Task<Result<object>> Handle(UpdateCouponRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateCouponValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var coupon = await couponRepository.FindByIdAsync(request.Id, true);
            if (coupon is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Coupon));

            var codeExisted = await couponRepository.FindSingleAsync(x => x.Code == request.Code, true);
            if (codeExisted is not null && coupon.Id != codeExisted.Id) AppleException.ThrowConflict("Code has existed in database.");

            var couponType = await couponTypeRepository.FindByIdAsync(request.CtId, true);
            if (couponType is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.CouponType));

            using var transaction = await couponRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                mapper.Map(request, coupon);
                coupon.Code = request.Code.ToUpper() ?? coupon.Code;
                couponRepository.Update(coupon);
                await couponRepository.SaveChangesAsync(cancellationToken);

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