using AppleShop.Application.Requests.ProductManagement.ProductImage;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductImage
{
    public class UpdateProductImageValidator : AbstractValidator<UpdateProductImageRequest>
    {
        public UpdateProductImageValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.")
                .NotNull().WithMessage("Id cannot be null.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
            RuleFor(x => x.Title).MaximumLength(128).WithMessage("Title cannot exceed 128 characters.");
            RuleFor(x => x.Position).GreaterThanOrEqualTo(0).WithMessage("Position must be at least 0.");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId must be greater than 0.");
            RuleFor(x => x.VariantId).GreaterThan(0).WithMessage("VariantId must be greater than 0.");
        }
    }
}