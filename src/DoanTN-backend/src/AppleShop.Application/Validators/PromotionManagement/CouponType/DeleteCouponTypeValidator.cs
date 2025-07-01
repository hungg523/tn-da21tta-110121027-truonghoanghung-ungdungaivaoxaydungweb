using AppleShop.Application.Requests.PromotionManagement.CouponType;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.CouponType
{
    public class DeleteCouponTypeValidator : AbstractValidator<DeleteCouponTypeRequest>
    {
        public DeleteCouponTypeValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}