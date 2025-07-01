using AppleShop.Application.Requests.ReviewManagement.ReviewReport;
using FluentValidation;

namespace AppleShop.Application.Validators.ReviewManagement.ReviewReport
{
    public class HandleReportValidator : AbstractValidator<HandleReportRequest>
    {
        public HandleReportValidator()
        {
            RuleFor(x => x.ReportId)
           .NotNull().WithMessage("ReportId must not be null.")
           .NotEmpty().WithMessage("ReportId must not be empty.")
           .GreaterThan(0).WithMessage("ReportId must be greater than 0.");

            RuleFor(x => x.Status)
           .NotNull().WithMessage("Status must not be null.")
           .NotEmpty().WithMessage("Status must not be empty.")
           .InclusiveBetween(1, 3).WithMessage("Status range from 1 to 3.");
        }
    }
}