using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductVariant
{
    public class UpdateProductVariantValidator : AbstractValidator<UpdateProductVariantRequest>
    {
        public UpdateProductVariantValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.")
                .NotNull().WithMessage("Id cannot be null.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId must be greater 0.");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be at least 0.");
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be at least 0.");
            RuleFor(x => x.IsActived).GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
        }
    }
}