namespace AppleShop.Application.Requests.DTOs.ReviewManagement.Review
{
    public class ReviewSummaryDTO
    {
        public int? VariantId { get; set; }
        public int? TotalReviews { get; set; }
        public double? AverageRating { get; set; }
        public RatingBreakdownDTO? RatingsBreakdown { get; set; }
    }

    public class RatingBreakdownDTO
    {
        public int? OneStar { get; set; } = 0;
        public int? TwoStar { get; set; } = 0;
        public int? ThreeStar { get; set; } = 0;
        public int? FourStar { get; set; } = 0;
        public int? FiveStar { get; set; } = 0;
    }
}