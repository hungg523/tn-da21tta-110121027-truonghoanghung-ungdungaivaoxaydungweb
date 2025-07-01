using AppleShop.Application.Requests.ProductManagement.ProductDetail;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductDetail
{
    public class CreateProductDetailValidator : AbstractValidator<CreateProductDetailRequest>
    {
        public CreateProductDetailValidator()
        {
            RuleFor(x => x.DetailKey).NotNull().WithMessage("Key cannot be null.").NotEmpty().WithMessage("Key cannot be empty.").MaximumLength(255).WithMessage("Key cannot exceed 255 characters.");
            RuleFor(x => x.DetailValue).NotNull().WithMessage("Value cannot be null.").NotEmpty().WithMessage("Value cannot be empty.").MaximumLength(1000).WithMessage("Value cannot exceed 1000 characters.");
            RuleFor(x => x.ProductId).NotNull().WithMessage("ProductId cannot be null.").NotEmpty().WithMessage("ProductId cannot be empty.").GreaterThan(0).WithMessage("ProductId must be greater than 0.");
        }
    }
}