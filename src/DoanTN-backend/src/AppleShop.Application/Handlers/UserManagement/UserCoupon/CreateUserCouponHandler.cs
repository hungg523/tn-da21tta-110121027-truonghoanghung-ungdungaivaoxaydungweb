using AppleShop.Application.Requests.UserManagement.UserCoupon;
using AppleShop.Application.Validators.UserManagement.UserCoupon;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.UserCoupon
{
    public class CreateUserCouponHandler : IRequestHandler<CreateUserCouponRequest, Result<object>>
    {
        private readonly ICouponRepository couponRepository;
        private readonly IUserRepository userRepository;
        private readonly IUserCouponRepository userCouponRepository;

        public CreateUserCouponHandler(ICouponRepository couponRepository, IUserRepository userRepository, IUserCouponRepository userCouponRepository)
        {
            this.couponRepository = couponRepository;
            this.userRepository = userRepository;
            this.userCouponRepository = userCouponRepository;
        }

        public async Task<Result<object>> Handle(CreateUserCouponRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateUserCouponValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var existedUserCoupon = await userCouponRepository.FindSingleAsync(x => x.CouponId == request.CouponId && x.UserId == request.UserId && x.IsUsed == false, true);
            if (existedUserCoupon is not null) AppleException.ThrowConflict("Voucher has been received.");

            var coupon = await couponRepository.FindSingleAsync(x => x.Id == request.CouponId);
            if (coupon is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Coupon));

            var user = await userRepository.FindSingleAsync(x => x.Id == request.UserId && x.IsActived == 1);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            using var transaction = await userCouponRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var userCoupon = new Entities.UserManagement.UserCoupon
                {
                    UserId = request.UserId,
                    CouponId = request.CouponId,
                    IsUsed = false,
                    TimesUsed = 0,
                    ClaimedAt = DateTime.Now,
                };
                userCouponRepository.Create(userCoupon);
                await userCouponRepository.SaveChangesAsync(cancellationToken);
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