using AppleShop.Application.Requests.ReviewManagement.Review;
using FluentValidation;

namespace AppleShop.Application.Validators.ReviewManagement.Review
{
    public class ReviewSumaryValidator : AbstractValidator<ReviewSumaryRequest>
    {
        public ReviewSumaryValidator()
        {
            RuleFor(x => x.VariantId)
            .NotNull().WithMessage("VariantId must not be null.")
            .NotEmpty().WithMessage("VariantId must not be empty.")
            .GreaterThan(0).WithMessage("VariantId must be greater than 0.");
        }
    }
}