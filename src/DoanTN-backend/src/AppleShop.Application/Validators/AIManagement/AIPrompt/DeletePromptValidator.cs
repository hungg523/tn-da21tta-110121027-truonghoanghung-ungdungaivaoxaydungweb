using AppleShop.Application.Requests.AIManagement.AIPrompt;
using FluentValidation;

namespace AppleShop.Application.Validators.AIManagement.AIPrompt
{
    public class DeletePromptValidator : AbstractValidator<DeletePromptRequest>
    {
        public DeletePromptValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().GreaterThan(0);
        }
    }
}