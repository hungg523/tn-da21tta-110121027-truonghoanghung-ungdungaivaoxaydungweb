using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.DTOs.PromotionManagement.ProductPromotion
{
    public class ProductPromotionDTO
    {
        public int? PmId { get; set; }
        
        [JsonIgnore]
        public int? ProductId { get; set; }

        [JsonIgnore]
        public string? ProductName { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
    }
}