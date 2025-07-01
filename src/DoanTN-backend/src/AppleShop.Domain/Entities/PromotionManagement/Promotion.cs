using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.PromotionManagement
{
    public class Promotion : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? DiscountAmout { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? IsActived { get; set; }
        public bool? IsFlashSale { get; set; }
        
        [JsonIgnore]
        public ICollection<ProductPromotion>? ProductPromotions { get; set; }
    }
}