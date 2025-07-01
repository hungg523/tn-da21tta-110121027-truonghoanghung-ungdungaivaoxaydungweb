using AppleShop.Application.Requests.ReturnManagement.Return;
using FluentValidation;

namespace AppleShop.Application.Validators.ReturnManagement.Return
{
    public class ChangeStatusReturnValidator : AbstractValidator<ChangeStatusReturnRequest>
    {
        public ChangeStatusReturnValidator()
        {
            RuleFor(x => x.Id)
            .NotNull().WithMessage("Id must not be null.")
            .NotEmpty().WithMessage("Id must not be empty.")
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.Status)
            .NotNull().WithMessage("Status must not be null.")
            .NotEmpty().WithMessage("Status must not be empty.")
            .GreaterThanOrEqualTo(0).WithMessage("Status must be greater than or equal 0.");
        }
    }
}