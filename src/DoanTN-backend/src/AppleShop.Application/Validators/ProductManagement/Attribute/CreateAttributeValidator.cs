using AppleShop.Application.Requests.ProductManagement.Attribute;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.Attribute
{
    public class CreateAttributeValidator : AbstractValidator<CreateAttributeRequest>
    {
        public CreateAttributeValidator()
        {
            RuleFor(x => x.Name).NotNull()
                .NotEmpty()
                .MaximumLength(64).WithMessage("Name cannot exceed 64 characters.");
        }
    }
}