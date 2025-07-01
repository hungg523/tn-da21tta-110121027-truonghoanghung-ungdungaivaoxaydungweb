using AppleShop.Application.Requests.PromotionManagement.CouponType;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.CouponType
{
    public class UpdateCouponTypeValidator : AbstractValidator<UpdateCouponTypeRequest>
    {
        public UpdateCouponTypeValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
            RuleFor(x => x.Name).GreaterThanOrEqualTo(0).WithMessage("Name must be greater than or equal 0.");
            RuleFor(x => x.Description).MaximumLength(255).WithMessage("Description cannot exceed 255 characters.");
        }
    }
}