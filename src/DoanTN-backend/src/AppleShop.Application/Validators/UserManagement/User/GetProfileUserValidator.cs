using AppleShop.Application.Requests.UserManagement.User;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.User
{
    public class GetProfileUserValidator : AbstractValidator<GetProfileUserRequest>
    {
        public GetProfileUserValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}