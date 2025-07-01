namespace AppleShop.Application.Requests.DTOs.StatisticalManagement.ProductReport
{
    public class TopProductReportDTO
    {
        public int? VariantId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? TotalQuantity { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public List<string>? ProductAttributes { get; set; }
    }
} 