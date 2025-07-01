using AppleShop.Application.Requests.ProductManagement.Product;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.Product
{
    public class CreateProductValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");
            RuleFor(x => x.Description).MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("CategoryId must be greater than 0.");
            RuleFor(x => x.IsActived).GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
        }
    }
}