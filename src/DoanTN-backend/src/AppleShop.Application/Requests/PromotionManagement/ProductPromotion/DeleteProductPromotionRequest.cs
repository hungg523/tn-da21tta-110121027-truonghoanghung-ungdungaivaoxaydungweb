using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.ProductPromotion
{
    public class DeleteProductPromotionRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}