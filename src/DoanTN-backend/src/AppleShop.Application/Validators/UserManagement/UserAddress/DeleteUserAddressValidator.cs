using AppleShop.Application.Requests.UserManagement.UserAddress;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.UserAddress
{
    public class DeleteUserAddressValidator : AbstractValidator<DeleteUserAddressRequest>
    {
        public DeleteUserAddressValidator()
        {
            RuleFor(c => c.Id)
                .NotNull().WithMessage("CustomerId cannot be null.")
                .GreaterThan(0).WithMessage("CustomerId must be greater than 0.");
        }
    }
}