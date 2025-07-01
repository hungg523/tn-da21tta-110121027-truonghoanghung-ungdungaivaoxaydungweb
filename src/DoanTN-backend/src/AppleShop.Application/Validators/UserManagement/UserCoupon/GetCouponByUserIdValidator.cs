using AppleShop.Application.Requests.UserManagement.UserCoupon;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.UserCoupon
{
    public class GetCouponByUserIdValidator : AbstractValidator<GetCouponByUserIdRequest>
    {
        public GetCouponByUserIdValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId must not be null.")
                .NotEmpty().WithMessage("UserId must not be empty.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");
        }
    }
}