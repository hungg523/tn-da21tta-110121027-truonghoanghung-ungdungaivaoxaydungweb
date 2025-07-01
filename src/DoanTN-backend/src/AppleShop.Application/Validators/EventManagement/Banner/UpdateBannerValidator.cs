using AppleShop.Application.Requests.EventManagement.Banner;
using FluentValidation;

namespace AppleShop.Application.Validators.EventManagement.Banner
{
    public class UpdateBannerValidator : AbstractValidator<UpdateBannerRequest>
    {
        public UpdateBannerValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.Title).MaximumLength(255);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Link).MaximumLength(500);
            RuleFor(x => x.Status).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        }
    }
}