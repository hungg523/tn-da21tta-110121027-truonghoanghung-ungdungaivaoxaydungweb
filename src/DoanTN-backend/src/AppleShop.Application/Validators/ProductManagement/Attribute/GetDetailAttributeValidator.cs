using AppleShop.Application.Requests.ProductManagement.Attribute;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.Attribute
{
    public class GetDetailAttributeValidator : AbstractValidator<GetDetailAttributeRequest>
    {
        public GetDetailAttributeValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.")
                .NotNull().WithMessage("Id cannot be null.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}