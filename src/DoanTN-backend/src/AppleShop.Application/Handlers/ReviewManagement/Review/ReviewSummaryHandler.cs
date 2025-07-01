using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;
using AppleShop.Application.Requests.ReviewManagement.Review;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ReviewManagement.Review
{
    public class ReviewSummaryHandler : IRequestHandler<ReviewSumaryRequest, Result<ReviewSummaryDTO>>
    {
        private readonly IReviewRepository reviewRepository;

        public ReviewSummaryHandler(IReviewRepository reviewRepository)
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task<Result<ReviewSummaryDTO>> Handle(ReviewSumaryRequest request, CancellationToken cancellationToken)
        {
            var reviews = reviewRepository.FindAll(x => x.VariantId == request.VariantId && x.IsDeleted == false).ToList();
            var totalReviews = reviews.Count;
            var totalStars = reviews.Sum(x => x.Rating);
            var averageRating = totalStars / (double)totalReviews;

            var reviewSummaryDto = new ReviewSummaryDTO
            {
                VariantId = request.VariantId,
                TotalReviews = totalReviews,
                AverageRating = averageRating,
                RatingsBreakdown = new RatingBreakdownDTO
                {
                    OneStar = reviews.Count(r => r.Rating == 1),
                    TwoStar = reviews.Count(r => r.Rating == 2),
                    ThreeStar = reviews.Count(r => r.Rating == 3),
                    FourStar = reviews.Count(r => r.Rating == 4),
                    FiveStar = reviews.Count(r => r.Rating == 5)
                }
            };

            return Result<ReviewSummaryDTO>.Ok(reviewSummaryDto);
        }
    }
}