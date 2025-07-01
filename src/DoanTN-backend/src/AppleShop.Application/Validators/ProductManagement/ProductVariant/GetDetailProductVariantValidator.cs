using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductVariant
{
    public class GetDetailProductVariantValidator : AbstractValidator<GetDetailProductVariantRequest>
    {
        public GetDetailProductVariantValidator()
        {
            RuleFor(x => x.VariantId).NotNull().WithMessage("Id must not be null.")
                 .NotEmpty().WithMessage("Id must not be empty")
                 .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}