using AppleShop.Application.Requests.AIManagement.AIPrompt;
using FluentValidation;

namespace AppleShop.Application.Validators.AIManagement.AIPrompt
{
    public class CreatePromptValidator : AbstractValidator<CreatePromptRequest>
    {
        public CreatePromptValidator()
        {
            RuleFor(x => x.Name).MaximumLength(128);
            RuleFor(x => x.Content).NotNull().NotEmpty().MaximumLength(1000);
        }
    }
}