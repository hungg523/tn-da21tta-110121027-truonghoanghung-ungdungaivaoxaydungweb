using AppleShop.Application.Requests.PromotionManagement.Coupon;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.Coupon
{
    public class DeleteCouponValidator : AbstractValidator<DeleteCouponRequest>
    {
        public DeleteCouponValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}