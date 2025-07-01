using AppleShop.Application.Requests.UserManagement.UserAddress;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.UserAddress
{
    public class GetAddressByUserIdValidator : AbstractValidator<GetAddressByUserIdRequest>
    {
        public GetAddressByUserIdValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("User Id must not be null.")
                .NotEmpty().WithMessage("User Id must not be empty.")
                .GreaterThan(0).WithMessage("User Id must be greater than 0.");
        }
    }
}