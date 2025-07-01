using AppleShop.Application.Requests.ReviewManagement.Review;
using FluentValidation;

namespace AppleShop.Application.Validators.ReviewManagement.Review
{
    public class CreateReviewValidator : AbstractValidator<CreateReviewRequest>
    {
        public CreateReviewValidator()
        {
            RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId must not be null.")
            .NotEmpty().WithMessage("UserId must not be empty.")
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.VariantId)
            .NotNull().WithMessage("VariantId must not be null.")
            .NotEmpty().WithMessage("VariantId must not be empty.")
            .GreaterThan(0).WithMessage("VariantId must be greater than 0.");

            RuleFor(x => x.Rating).NotNull().WithMessage("Rating cannot be null.")
                .NotEmpty().WithMessage("Rating cannot be empty.")
                .InclusiveBetween(1, 5).WithMessage("Ratings range from 1 to 5.");

            RuleFor(x => x.Comment)
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
        }
    }
}