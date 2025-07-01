using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Promotion
{
    public class UpdatePromotionRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? DiscountPercentage { get; set; }
        public int? DiscountAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? IsActived { get; set; }
        public bool? IsFlashSale { get; set; }
    }
}