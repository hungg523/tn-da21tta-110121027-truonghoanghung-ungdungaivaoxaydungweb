using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.ProductPromotion
{
    public class UpdateProductPromotionValidator : AbstractValidator<UpdateProductPromotionRequest>
    {
        public UpdateProductPromotionValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.").NotEmpty().WithMessage("Id must not be empty.").GreaterThan(0).WithMessage("Id must be greater than 0.");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId must be greater than 0.");
            RuleFor(x => x.VariantId).GreaterThan(0).WithMessage("VariantId must be greater than 0.");
            RuleFor(x => x.PromotionId).GreaterThan(0).WithMessage("PromotionId must be greater than 0.");
        }
    }
}