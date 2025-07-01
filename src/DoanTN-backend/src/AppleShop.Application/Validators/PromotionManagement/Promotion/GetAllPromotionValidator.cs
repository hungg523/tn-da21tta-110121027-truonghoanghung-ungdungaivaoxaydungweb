using AppleShop.Application.Requests.PromotionManagement.Promotion;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.Promotion
{
    public class GetAllPromotionValidator : AbstractValidator<GetAllPromotionRequest>
    {
        public GetAllPromotionValidator()
        {
            RuleFor(x => x.IsActived).GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
        }
    }
}