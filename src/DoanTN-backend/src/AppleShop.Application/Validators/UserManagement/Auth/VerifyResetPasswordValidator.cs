using AppleShop.Application.Requests.UserManagement.Auth;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.Auth
{
    public class VerifyResetPasswordValidator : AbstractValidator<VerifyResetPasswordRequest>
    {
        public VerifyResetPasswordValidator()
        {
            RuleFor(u => u.Email)
                .NotNull().WithMessage("Email cannot be null.")
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");
        }
    }
}