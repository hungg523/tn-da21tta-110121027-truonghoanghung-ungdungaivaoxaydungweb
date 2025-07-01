using AppleShop.Application.Requests.ReturnManagement.Return;
using FluentValidation;

namespace AppleShop.Application.Validators.ReturnManagement.Return
{
    public class GetAllReturnValidator : AbstractValidator<GetAllReturnRequest>
    {
        public GetAllReturnValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("UserId must be greater than 0.");
        }
    }
}