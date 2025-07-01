using AppleShop.Application.Requests.ReviewManagement.Review;
using FluentValidation;

namespace AppleShop.Application.Validators.ReviewManagement.Review
{
    public class ReplyReviewByAdminValidator : AbstractValidator<ReplyReviewByAdminRequest>
    {
        public ReplyReviewByAdminValidator()
        {
            RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId must not be null.")
            .NotEmpty().WithMessage("UserId must not be empty.")
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.ReviewId)
            .NotNull().WithMessage("ReviewId must not be null.")
            .NotEmpty().WithMessage("ReviewId must not be empty.")
            .GreaterThan(0).WithMessage("ReviewId must be greater than 0.");

            RuleFor(x => x.ReplyText)
                .NotNull().WithMessage("ReplyText must not be null.")
                .NotEmpty().WithMessage("ReplyText must not be empty.")
                .MaximumLength(500).WithMessage("ReplyText cannot exceed 500 characters.");
        }
    }
}