using AppleShop.Application.Requests.PromotionManagement.CouponType;
using AppleShop.Application.Validators.PromotionManagement.CouponType;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.CouponType
{
    public class DeleteCouponTypeHandler : IRequestHandler<DeleteCouponTypeRequest, Result<object>>
    {
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly ICouponRepository couponRepository;

        public DeleteCouponTypeHandler(ICouponTypeRepository couponTypeRepository, ICouponRepository couponRepository)
        {
            this.couponTypeRepository = couponTypeRepository;
            this.couponRepository = couponRepository;
        }

        public async Task<Result<object>> Handle(DeleteCouponTypeRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteCouponTypeValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var couponType = await couponTypeRepository.FindByIdAsync(request.Id, true);
            if (couponType is not null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.CouponType));

            var coupon = couponRepository.FindAll(x => x.CtId == request.Id).ToList();
            if (coupon is not null) AppleException.ThrowConflict("Coupon Type has been used by Coupon.");

            using var transaction = await couponTypeRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                couponTypeRepository.Delete(couponType);
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