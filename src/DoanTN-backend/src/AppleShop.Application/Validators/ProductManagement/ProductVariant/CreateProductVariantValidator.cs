using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductVariant
{
    public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantRequest>
    {
        public CreateProductVariantValidator()
        {
            RuleFor(x => x.ProductId).NotNull().WithMessage("ProductId is required.")
                .GreaterThan(0).WithMessage("ProductId must be greater 0.");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be at least 0.");
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be at least 0.");
            RuleFor(x => x.IsActived).GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
        }
    }
}