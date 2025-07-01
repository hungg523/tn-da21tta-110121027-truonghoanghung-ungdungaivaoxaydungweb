using AppleShop.Application.Requests.AIManagement.AIPrompt;
using FluentValidation;

namespace AppleShop.Application.Validators.AIManagement.AIPrompt
{
    public class UpdatePromptValidator : AbstractValidator<UpdatePromptRequest>
    {
        public UpdatePromptValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.Name).MaximumLength(128);
            RuleFor(x => x.Content).MaximumLength(1000);
        }
    }
}