using AppleShop.Application.Requests.UserManagement.Auth;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.Auth
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordValidator()
        {
            RuleFor(u => u.NewPassword)
                .NotNull().WithMessage("Password cannot be null.")
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MaximumLength(255).WithMessage("Password cannot exceed 255 characters.");
        }
    }
}