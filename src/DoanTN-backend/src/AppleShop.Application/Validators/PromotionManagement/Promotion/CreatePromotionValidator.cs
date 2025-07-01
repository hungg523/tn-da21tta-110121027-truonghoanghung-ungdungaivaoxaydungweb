using AppleShop.Application.Requests.PromotionManagement.Promotion;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.Promotion
{
    public class CreatePromotionValidator : AbstractValidator<CreatePromotionRequest>
    {
        public CreatePromotionValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("Code must not be null.")
                .NotEmpty().WithMessage("Code must be empty.")
                .MaximumLength(128).WithMessage("Code cannot exceed 128 characters.");
            RuleFor(x => x.Description).MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
            RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100.");
            RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0).WithMessage("Discount amount must be greater than 0.");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be greater than or equal to the start date.");

            RuleFor(x => x.IsActived).GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than 0.");
        }
    }
}