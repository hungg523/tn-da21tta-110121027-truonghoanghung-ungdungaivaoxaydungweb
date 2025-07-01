using AppleShop.Application.Requests.CategoryManagement.Category;
using FluentValidation;

namespace AppleShop.Application.Validators.CategoryManagement.Category
{
    public class GetDetailCategoryValidator : AbstractValidator<GetDetailCategoryRequest>
    {
        public GetDetailCategoryValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                 .NotEmpty().WithMessage("Id must not be empty")
                 .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}