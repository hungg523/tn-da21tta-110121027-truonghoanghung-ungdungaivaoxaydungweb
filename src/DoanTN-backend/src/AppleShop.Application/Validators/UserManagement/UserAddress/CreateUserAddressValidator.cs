using AppleShop.Application.Requests.UserManagement.UserAddress;
using FluentValidation;

namespace AppleShop.Application.Validators.UserManagement.UserAddress
{
    public class CreateUserAddressValidator : AbstractValidator<CreateUserAddressRequest>
    {
        public CreateUserAddressValidator()
        {
            RuleFor(c => c.UserId)
                .NotNull().WithMessage("User Id cannot be null.")
                .GreaterThan(0).WithMessage("User Id must be greater than 0.");

            RuleFor(c => c.AddressLine)
                .NotNull().WithMessage("Address Line cannot be null.")
                .NotEmpty().WithMessage("Address Line cannot be empty.")
                .MaximumLength(255).WithMessage("Address Line must not exceed 255 characters.");

            RuleFor(c => c.FirstName)
                .NotNull().WithMessage("First Name cannot be null.")
                .NotEmpty().WithMessage("First Name cannot be empty.")
                .MaximumLength(255).WithMessage("First Name must not exceed 255 characters.");

            RuleFor(c => c.LastName)
                .NotNull().WithMessage("Last Name cannot be null.")
                .NotEmpty().WithMessage("Last Name cannot be empty.")
                .MaximumLength(255).WithMessage("Last Name must not exceed 255 characters.");

            RuleFor(c => c.PhoneNumber)
                .NotNull().WithMessage("Phone cannot be null.")
                .NotEmpty().WithMessage("Phone cannot be empty.")
                .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

            RuleFor(c => c.Province)
                .NotNull().WithMessage("Province cannot be null.")
                .NotEmpty().WithMessage("Province cannot be empty.")
                .MaximumLength(100).WithMessage("Province must not exceed 100 characters.");

            RuleFor(c => c.District)
                .NotNull().WithMessage("District cannot be null.")
                .NotEmpty().WithMessage("District cannot be empty.")
                .MaximumLength(100).WithMessage("District must not exceed 100 characters.");

            RuleFor(c => c.Ward)
                .NotNull().WithMessage("Ward cannot be null.")
                .NotEmpty().WithMessage("Ward cannot be empty.")
                .MaximumLength(100).WithMessage("Ward must not exceed 100 characters.");
        }
    }
}