namespace AppleShop.Application.Requests.DTOs.ReviewManagement.Review
{
    public class ProductReviewDTO
    {
        public int? VariantId { get; set; }
        public List<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();
    }
}