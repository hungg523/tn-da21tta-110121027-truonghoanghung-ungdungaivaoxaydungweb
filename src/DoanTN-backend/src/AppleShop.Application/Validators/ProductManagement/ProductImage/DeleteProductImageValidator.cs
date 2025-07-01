using AppleShop.Application.Requests.ProductManagement.ProductImage;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductImage
{
    public class DeleteProductImageValidator : AbstractValidator<DeleteProductImageRequest>
    {
        public DeleteProductImageValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.")
                .NotNull().WithMessage("Id cannot be null.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}