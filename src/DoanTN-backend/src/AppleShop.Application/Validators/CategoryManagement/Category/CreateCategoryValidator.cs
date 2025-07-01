using AppleShop.Application.Requests.CategoryManagement.Category;
using FluentValidation;

namespace AppleShop.Application.Validators.CategoryManagement.Category
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name)
            .NotNull().WithMessage("Name must not be null.")
            .NotEmpty().WithMessage("Name must not be empty.")
            .MaximumLength(255).WithMessage("Name must be less than or equal to 255 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(int.MaxValue).WithMessage("Description length is too large.");

            RuleFor(x => x.IsActived)
                .GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
        }
    }
}