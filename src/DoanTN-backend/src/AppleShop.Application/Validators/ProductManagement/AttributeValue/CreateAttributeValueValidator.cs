using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.AttributeValue
{
    public class CreateAttributeValueValidator : AbstractValidator<CreateAttributeValueRequest>
    {
        public CreateAttributeValueValidator()
        {
            RuleFor(x => x.AttributeId).NotNull().WithMessage("AttributeId is required.")
                .GreaterThan(0).WithMessage("AttributeId must be greater 0.");
            RuleFor(x => x.Value)
                .NotNull().WithMessage("Value cannot be null.")
                .NotEmpty().WithMessage("Value cannot be empty.")
                .MaximumLength(64).WithMessage("Value cannot exceed 64 characters.");
        }
    }
}