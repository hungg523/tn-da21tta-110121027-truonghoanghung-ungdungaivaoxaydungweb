using AppleShop.Application.Requests.UserManagement.Auth;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.Auth
{
    public class RegisterValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterValidator()
        {
            RuleFor(u => u.Email)
                .NotNull().WithMessage("Email cannot be null.")
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

            RuleFor(u => u.Password)
                .NotNull().WithMessage("Password cannot be null.")
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MaximumLength(255).WithMessage("Password cannot exceed 255 characters.");

            RuleFor(u => u.ConfirmPassword)
                .NotNull().WithMessage("ConfirmPassword cannot be null.")
                .NotEmpty().WithMessage("ConfirmPassword cannot be empty.")
                .Equal(u => u.Password).WithMessage("ConfirmPassword must match Password.");
        }
    }
}