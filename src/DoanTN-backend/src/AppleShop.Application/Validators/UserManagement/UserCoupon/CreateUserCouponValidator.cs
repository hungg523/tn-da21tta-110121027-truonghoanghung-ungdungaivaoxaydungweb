using AppleShop.Application.Requests.UserManagement.UserCoupon;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.UserCoupon
{
    public class CreateUserCouponValidator : AbstractValidator<CreateUserCouponRequest>
    {
        public CreateUserCouponValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId must not be null.")
                .NotEmpty().WithMessage("UserId must not be empty.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.CouponId).NotNull().WithMessage("CouponId must not be null.")
                .NotEmpty().WithMessage("CouponId must not be empty.")
                .GreaterThan(0).WithMessage("CouponId must be greater than 0.");
        }
    }
}