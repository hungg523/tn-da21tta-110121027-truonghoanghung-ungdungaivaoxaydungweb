using AppleShop.Application.Requests.OrderManagement.Order;
using FluentValidation;

namespace AppleShop.Application.Validators.OrderManagement.Order
{
    public class GetAllOrderValidator : AbstractValidator<GetAllOrderRequest>
    {
        public GetAllOrderValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("UserId must be greater than 0.");
        }
    }
}