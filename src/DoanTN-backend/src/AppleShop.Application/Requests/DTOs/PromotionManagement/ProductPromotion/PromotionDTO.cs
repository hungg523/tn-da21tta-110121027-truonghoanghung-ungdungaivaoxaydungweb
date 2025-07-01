namespace AppleShop.Application.Requests.DTOs.PromotionManagement.ProductPromotion
{
    public class PromotionDTO
    {
        public int? PromotionId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? DiscountAmount { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? IsActived { get; set; }
        public bool? IsFlashSale { get; set; }
        public List<ProductPromotionDTO> Promotions { get; set; } = new List<ProductPromotionDTO>();
    }
}