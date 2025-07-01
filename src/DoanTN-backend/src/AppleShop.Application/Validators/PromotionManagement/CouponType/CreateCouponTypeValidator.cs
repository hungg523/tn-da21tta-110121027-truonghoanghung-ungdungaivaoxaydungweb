using AppleShop.Application.Requests.PromotionManagement.CouponType;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.CouponType
{
    public class CreateCouponTypeValidator : AbstractValidator<CreateCouponTypeRequest>
    {
        public CreateCouponTypeValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("Name must not be null.")
                .NotEmpty().WithMessage("Name must be empty.")
                .GreaterThanOrEqualTo(0).WithMessage("Name must be greater than or equal 0.");
            RuleFor(x => x.Description).MaximumLength(255).WithMessage("Description cannot exceed 255 characters.");
        }
    }
}