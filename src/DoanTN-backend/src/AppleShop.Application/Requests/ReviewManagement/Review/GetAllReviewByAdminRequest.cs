using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ReviewManagement.Review
{
    public class GetAllReviewByAdminRequest : IQuery<List<ProductReviewDTO>>
    {
        public int? VariantId { get; set; }
        public int? Star { get; set; }
    }
}