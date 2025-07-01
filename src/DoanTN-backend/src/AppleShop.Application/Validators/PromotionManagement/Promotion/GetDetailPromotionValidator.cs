using AppleShop.Application.Requests.PromotionManagement.Promotion;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.Promotion
{
    public class GetDetailPromotionValidator : AbstractValidator<GetDetailPromotionRequest>
    {
        public GetDetailPromotionValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}