using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.AttributeValue
{
    public class GetDetailAttributeValueValidator : AbstractValidator<GetDetailAttributeValueRequest>
    {
        public GetDetailAttributeValueValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id is required.")
                .GreaterThan(0).WithMessage("Id must be greater 0.");
        }
    }
}