using AppleShop.Application.Requests.OrderManagement.Order;
using FluentValidation;

namespace AppleShop.Application.Validators.OrderManagement.Order
{
    public class GetOrderDetailValidator : AbstractValidator<GetOrderDetailRequest>
    {
        public GetOrderDetailValidator()
        {
            RuleFor(x => x.OrderId).NotNull().WithMessage("Id must not be null.")
                 .NotEmpty().WithMessage("Id must not be empty")
                 .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}