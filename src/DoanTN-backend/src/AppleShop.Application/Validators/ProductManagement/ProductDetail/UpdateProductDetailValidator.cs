using AppleShop.Application.Requests.ProductManagement.ProductDetail;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductDetail
{
    internal class UpdateProductDetailValidator : AbstractValidator<UpdateProductDetailRequest>
    {
        public UpdateProductDetailValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.")
                .NotNull().WithMessage("Id cannot be null.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
            RuleFor(x => x.DetailKey).MaximumLength(255).WithMessage("Key cannot exceed 255 characters.");
            RuleFor(x => x.DetailValue).MaximumLength(1000).WithMessage("Value cannot exceed 1000 characters.");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId must be greater than 0.");
        }
    }
}