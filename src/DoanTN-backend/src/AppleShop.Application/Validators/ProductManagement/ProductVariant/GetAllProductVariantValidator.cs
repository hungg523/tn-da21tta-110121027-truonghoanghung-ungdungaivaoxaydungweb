using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using FluentValidation;

namespace AppleShop.Application.Validators.ProductManagement.ProductVariant
{
    public class GetAllProductVariantValidator : AbstractValidator<GetAllProductVariantRequest>
    {
        public GetAllProductVariantValidator()
        {
            RuleFor(x => x.IsActived).GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("CategoryId must be greater than 0.");
        }
    }
}