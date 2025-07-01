using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.ProductPromotion
{
    public class UpdateProductPromotionRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? ProductId { get; set; }
        public int? VariantId { get; set; }
        public int? PromotionId { get; set; }
    }
}