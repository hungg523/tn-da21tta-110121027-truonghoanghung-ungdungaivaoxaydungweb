using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.ProductPromotion
{
    public class CreateProductPromotionValidator : AbstractValidator<CreateProductPromotionRequest>
    {
        public CreateProductPromotionValidator()
        {
            RuleFor(x => x.VariantIds).Must(list => list.All(id => id > 0)).WithMessage("VariantId not match condition.");
            RuleFor(x => x.ProductIds).Must(list => list.All(id => id > 0)).WithMessage("ProductId not match condition.");
            RuleFor(x => x.PromotionId).NotNull().WithMessage("PromotionId must not be null.").NotEmpty().WithMessage("PromotionId must not be empty.").GreaterThan(0).WithMessage("PromotionId must be greater than 0.");
        }
    }
}