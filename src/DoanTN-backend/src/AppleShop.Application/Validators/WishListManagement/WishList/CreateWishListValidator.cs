using AppleShop.Application.Requests.WishListManagement.WishList;
using FluentValidation;

namespace AppleShop.Application.Validators.WishListManagement.WishList
{
    public class CreateWishListValidator : AbstractValidator<CreateWishListRequest>
    {
        public CreateWishListValidator()
        {
            RuleFor(x => x.UserId).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.VariantId).NotNull().NotEmpty().GreaterThan(0);
        }
    }
}