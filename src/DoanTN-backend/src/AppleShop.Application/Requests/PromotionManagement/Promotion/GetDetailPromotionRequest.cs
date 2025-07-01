using Entities = AppleShop.Domain.Entities;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Promotion
{
    public class GetDetailPromotionRequest : IQuery<Entities.PromotionManagement.Promotion>
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}