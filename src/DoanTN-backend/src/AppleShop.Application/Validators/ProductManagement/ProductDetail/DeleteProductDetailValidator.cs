using AppleShop.Application.Requests.ProductManagement.ProductDetail;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductDetail
{
    public class DeleteProductDetailValidator : AbstractValidator<DeleteProductDetailRequest>
    {
        public DeleteProductDetailValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.")
                .NotNull().WithMessage("Id cannot be null.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}