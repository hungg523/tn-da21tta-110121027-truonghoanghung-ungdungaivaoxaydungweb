using AppleShop.Application.Requests.OrderManagement.Order;
using FluentValidation;

namespace AppleShop.Application.Validators.OrderManagement.Order
{
    public class ChangeOrderStatusValidator : AbstractValidator<ChangeOrderStatusRequest>
    {
        public ChangeOrderStatusValidator()
        {
            RuleFor(x => x.OiId)
                .NotNull().WithMessage("OrderItemId must not be null.")
                .NotEmpty().WithMessage("OrderItemId must not be empty.")
                .GreaterThan(0).WithMessage("OrderItemId must be greater than 0.");
        }
    }
}