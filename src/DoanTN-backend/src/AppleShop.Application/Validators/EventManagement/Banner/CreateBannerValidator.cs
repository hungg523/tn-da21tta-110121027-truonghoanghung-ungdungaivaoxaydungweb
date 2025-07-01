using AppleShop.Application.Requests.EventManagement.Banner;
using FluentValidation;

namespace AppleShop.Application.Validators.EventManagement.Banner
{
    public class CreateBannerValidator : AbstractValidator<CreateBannerRequest>
    {
        public CreateBannerValidator()
        {
            RuleFor(x => x.Title).NotNull().NotEmpty().MaximumLength(255);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Link).MaximumLength(500);
            RuleFor(x => x.StartDate).NotNull().NotEmpty();
            RuleFor(x => x.EndDate).NotNull().NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
            RuleFor(x => x.Status).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        }
    }
}