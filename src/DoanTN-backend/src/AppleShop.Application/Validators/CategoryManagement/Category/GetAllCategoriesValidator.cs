using AppleShop.Application.Requests.CategoryManagement.Category;
using FluentValidation;

namespace AppleShop.Application.Validators.CategoryManagement.Category
{
    public class GetAllCategoriesValidator : AbstractValidator<GetAllCategoriesRequest>
    {
        public GetAllCategoriesValidator()
        {
            RuleFor(x => x.IsActived).GreaterThanOrEqualTo(0).WithMessage("IsActived must be greater than or equal 0.");
        }
    }
}