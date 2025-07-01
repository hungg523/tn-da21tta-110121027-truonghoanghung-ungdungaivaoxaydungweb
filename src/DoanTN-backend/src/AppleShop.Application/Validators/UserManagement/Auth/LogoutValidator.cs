using AppleShop.Application.Requests.UserManagement.Auth;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.Auth
{
    public class LogoutValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutValidator()
        {
            RuleFor(u => u.UserId).NotNull().NotEmpty().GreaterThan(0);
        }
    }
}