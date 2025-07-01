using AppleShop.Application.Requests.OrderManagement.Cart;
using FluentValidation;

namespace AppleShop.Application.Validators.OrderManagement.Cart
{
    public class DeleteProductFromCartValidator : AbstractValidator<DeleteProductFromCartRequest>
    {
        public DeleteProductFromCartValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId must not be null.")
                .NotEmpty().WithMessage("UserId must not be empty.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.VariantId)
                .NotNull().WithMessage("VariantId must not be null.")
                .NotEmpty().WithMessage("VariantId must not be empty.")
                .GreaterThan(0).WithMessage("VariantId must be greater than 0.");
        }
    }
}