using AppleShop.Application.Requests.PromotionManagement.Coupon;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.Coupon
{
    public class UpdateCouponValidator : AbstractValidator<UpdateCouponRequest>
    {
        public UpdateCouponValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
            RuleFor(x => x.CtId).NotNull().WithMessage("Coupon Type Id must not be null.")
                .NotEmpty().WithMessage("Coupon Type Id must not be empty.")
                .GreaterThan(0).WithMessage("Coupon Type Id must be greater than 0.");
            RuleFor(x => x.Code).NotNull().MaximumLength(10).WithMessage("Code cannot exceed 10 characters.");
            RuleFor(x => x.Description).MaximumLength(512).WithMessage("Description cannot exceed 512 characters.");
            RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100.");
            RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0).WithMessage("Discount amount must be greater than or equal 0.");
            RuleFor(x => x.MaxDiscountAmount).GreaterThanOrEqualTo(0).WithMessage("Discount amount must be greater than or equal 0.");
            RuleFor(x => x.MinOrderValue).GreaterThanOrEqualTo(0).WithMessage("Discount amount must be greater than or equal 0.");
            RuleFor(x => x.MaxUsage).GreaterThanOrEqualTo(0).WithMessage("Max Used must be greater than 0.");
            RuleFor(x => x.MaxUsagePerUser).GreaterThanOrEqualTo(0).WithMessage("Max usage per user must be greater than 0.");
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to the start date.");
        }
    }
}