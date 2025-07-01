using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.ProductPromotion
{
    public class GetDetailProductPromotionValidator : AbstractValidator<GetDetailProductPromotionRequest>
    {
        public GetDetailProductPromotionValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.").NotEmpty().WithMessage("Id must not be empty.").GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}