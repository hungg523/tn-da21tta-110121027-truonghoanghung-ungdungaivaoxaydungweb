using AppleShop.Application.Requests.ProductManagement.Product;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.Product
{
    public class GetProductDetailValidator : AbstractValidator<GetProductDetailRequest>
    {
        public GetProductDetailValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                 .NotEmpty().WithMessage("Id must not be empty")
                 .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}