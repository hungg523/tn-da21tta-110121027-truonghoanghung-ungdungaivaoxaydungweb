using AppleShop.Application.Requests.OrderManagement.Cart;
using FluentValidation;

namespace AppleShop.Application.Validators.OrderManagement.Cart
{
    public class GetDetailCartValidator : AbstractValidator<GetDetailCartRequest>
    {
        public GetDetailCartValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("Id must not be null.")
                 .NotEmpty().WithMessage("Id must not be empty")
                 .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}