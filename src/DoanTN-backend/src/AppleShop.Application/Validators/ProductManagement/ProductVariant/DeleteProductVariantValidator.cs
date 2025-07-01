using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductVariant
{
    public class DeleteProductVariantValidator : AbstractValidator<DeleteProductVariantRequest>
    {
        public DeleteProductVariantValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.")
                .NotNull().WithMessage("Id cannot be null.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}