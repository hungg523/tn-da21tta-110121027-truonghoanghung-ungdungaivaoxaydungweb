using AppleShop.Application.Requests.CategoryManagement.Category;
using FluentValidation;

namespace AppleShop.Application.Validators.CategoryManagement.Category
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id must not be null.")
                .NotEmpty().WithMessage("Id must not be empty")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.CatPid).GreaterThanOrEqualTo(0);

            RuleFor(x => x.Name)
           .MaximumLength(255).WithMessage("Name must be less than or equal to 255 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(int.MaxValue).WithMessage("Description length is too large.");

            RuleFor(x => x.IsActived)
                .GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
        }
    }
}