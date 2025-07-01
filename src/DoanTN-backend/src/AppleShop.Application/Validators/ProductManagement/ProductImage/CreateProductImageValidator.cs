using AppleShop.Application.Requests.ProductManagement.ProductImage;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductImage
{
    public class CreateProductImageValidator : AbstractValidator<CreateProductImageRequest>
    {
        public CreateProductImageValidator()
        {
            RuleFor(x => x.Title).MaximumLength(128).WithMessage("Title cannot exceed 128 characters.");
            RuleFor(x => x.Position).GreaterThanOrEqualTo(0).WithMessage("Position must be at least 0.");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId must be greater than 0.");
            RuleFor(x => x.VariantId).GreaterThan(0).WithMessage("VariantId must be greater than 0.");
        }
    }
}