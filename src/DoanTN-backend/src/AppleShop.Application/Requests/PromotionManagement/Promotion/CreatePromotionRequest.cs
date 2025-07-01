using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.PromotionManagement.Promotion
{
    public class CreatePromotionRequest : ICommand
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? DiscountPercentage { get; set; } = 0;
        public int? DiscountAmount { get; set; } = 0;
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;
        public int? IsActived { get; set; } = 1;
        public bool? IsFlashSale { get; set; } = false;
    }
}