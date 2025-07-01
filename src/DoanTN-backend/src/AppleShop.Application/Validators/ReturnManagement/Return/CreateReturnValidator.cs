using AppleShop.Application.Requests.ReturnManagement.Return;
using FluentValidation;

namespace AppleShop.Application.Validators.ReturnManagement.Return
{
    public class CreateReturnValidator : AbstractValidator<CreateReturnRequest>
    {
        public CreateReturnValidator()
        {
            RuleFor(x => x.OrderItemId)
            .NotNull().WithMessage("OrderItemId must not be null.")
            .NotEmpty().WithMessage("OrderItemId must not be empty.")
            .GreaterThan(0).WithMessage("OrderItemId must be greater than 0.");

            RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId must not be null.")
            .NotEmpty().WithMessage("UserId must not be empty.")
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Quantity must not be null.")
            .NotEmpty().WithMessage("Quantity must not be empty.")
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Reason)
                .NotNull().WithMessage("Reason must not be null.")
                .NotEmpty().WithMessage("Reason must not be empty.")
                .MaximumLength(255).WithMessage("Reason cannot exceed 255 characters.");
        }
    }
}