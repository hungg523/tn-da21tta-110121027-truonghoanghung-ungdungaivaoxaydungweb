using AppleShop.Application.Requests.PromotionManagement.Coupon;
using FluentValidation;

namespace AppleShop.Application.Validators.PromotionManagement.Coupon
{
    public class GetDetailCouponValidator : AbstractValidator<GetDetailCouponRequest>
    {
        public GetDetailCouponValidator()
        {
            RuleFor(x => x.Code).NotNull().NotEmpty();
        }
    }
}