using AppleShop.Application.Requests.ReviewManagement.ReviewReport;
using FluentValidation;

namespace AppleShop.Application.Validators.ReviewManagement.ReviewReport
{
    public class ReportReviewValidator : AbstractValidator<ReportReviewRequest>
    {
        public ReportReviewValidator()
        {
            RuleFor(x => x.ReviewId)
            .NotNull().WithMessage("ReviewId must not be null.")
            .NotEmpty().WithMessage("ReviewId must not be empty.")
            .GreaterThan(0).WithMessage("ReviewId must be greater than 0.");

            RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId must not be null.")
            .NotEmpty().WithMessage("UserId must not be empty.")
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.Reason)
            .NotNull().WithMessage("Reason must not be null.")
            .NotEmpty().WithMessage("Reason must not be empty.")
            .MaximumLength(300).WithMessage("Reason cannot exceed 300 characters.");
        }
    }
}