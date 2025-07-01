using AppleShop.Application.Requests.UserManagement.UserAddress;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.UserAddress
{
    public class SetDefaultAddressValidator : AbstractValidator<SetDefaultAddressRequest>
    {
        public SetDefaultAddressValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.UserId).NotNull().WithMessage("UserId must not be null.")
                .NotEmpty().WithMessage("UserId must not be empty.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");
        }
    }
}