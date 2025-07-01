using AppleShop.Application.Requests.UserManagement.Auth;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.Auth
{
    public class VerifyOTPValidator : AbstractValidator<VerifyOTPRequest>
    {
        public VerifyOTPValidator()
        {
            RuleFor(u => u.Email)
                .NotNull().WithMessage("Email cannot be null.")
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

            RuleFor(x => x.OTP).Length(6).WithMessage("OTP must be exceed 6 characters.");
        }
    }
}