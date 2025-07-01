using Entities = AppleShop.Domain.Entities;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.ProductPromotion
{
    public class GetDetailProductPromotionRequest : IQuery<Entities.PromotionManagement.ProductPromotion>
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}