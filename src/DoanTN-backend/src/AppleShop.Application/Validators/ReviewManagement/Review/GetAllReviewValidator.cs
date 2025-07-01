using AppleShop.Application.Requests.ReviewManagement.Review;
using FluentValidation;

namespace AppleShop.Application.Validators.ReviewManagement.Review
{
    public class GetAllReviewValidator : AbstractValidator<GetAllReviewRequest>
    {
        public GetAllReviewValidator()
        {
            RuleFor(x => x.VariantId).GreaterThan(0).WithMessage("VariantId must be greater than 0.");
            RuleFor(x => x.Star).InclusiveBetween(1, 5).WithMessage("Star range from 1 to 5.");
        }
    }
}