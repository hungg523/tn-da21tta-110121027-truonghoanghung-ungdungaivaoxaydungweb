using AppleShop.Application.Requests.WishListManagement.WishList;
using FluentValidation;

namespace AppleShop.Application.Validators.WishListManagement.WishList
{
    public class ChangeStatusWishListValidator : AbstractValidator<ChangeStatusWishListRequest>
    {
        public ChangeStatusWishListValidator()
        {
            RuleFor(x => x.VariantId).NotNull().NotEmpty().GreaterThan(0);
        }
    }
}