using AppleShop.Application.Requests.EventManagement.Banner;
using FluentValidation;

namespace AppleShop.Application.Validators.EventManagement.Banner
{
    public class DeleteBannerValidator : AbstractValidator<DeleteBannerRequest>
    {
        public DeleteBannerValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().GreaterThan(0);
        }
    }
}