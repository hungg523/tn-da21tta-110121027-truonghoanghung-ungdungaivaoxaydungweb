using AppleShop.Application.Requests.UserManagement.Auth;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.Auth
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(u => u.UserId).NotNull().NotEmpty().GreaterThan(0);

            RuleFor(u => u.NewPassword)
                .NotNull().WithMessage("Password cannot be null.")
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MaximumLength(255).WithMessage("Password cannot exceed 255 characters.");

            RuleFor(u => u.ConfirmPassword)
                .NotNull().WithMessage("ConfirmPassword cannot be null.")
                .NotEmpty().WithMessage("ConfirmPassword cannot be empty.")
                .Equal(u => u.NewPassword).WithMessage("ConfirmPassword must match Password.");
        }
    }
}