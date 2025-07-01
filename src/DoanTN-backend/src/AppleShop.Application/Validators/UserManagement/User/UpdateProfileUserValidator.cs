using AppleShop.Application.Requests.UserManagement.User;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.User
{
    public class UpdateProfileUserValidator : AbstractValidator<UpdateProfileUserRequest>
    {
        public UpdateProfileUserValidator()
        {
            RuleFor(c => c.Id)
                .NotNull().WithMessage("CustomerId cannot be null.")
                .GreaterThan(0).WithMessage("CustomerId must be greater than 0.");
            RuleFor(x => x.UserName).MaximumLength(255).WithMessage("Username must not exceed 255 characters.");
            RuleFor(x => x.Gender).MaximumLength(32).WithMessage("Gender must not exceed 32 characters.");
        }
    }
}