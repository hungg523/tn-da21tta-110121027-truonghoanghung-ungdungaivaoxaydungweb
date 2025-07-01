using AppleShop.Application.Requests.EventManagement.Banner;
using FluentValidation;

namespace AppleShop.Application.Validators.EventManagement.Banner
{
    public class GetAllBannerValidator : AbstractValidator<GetAllBannerRequest>
    {
        public GetAllBannerValidator()
        {
            RuleFor(x => x.Status).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        }
    }
}