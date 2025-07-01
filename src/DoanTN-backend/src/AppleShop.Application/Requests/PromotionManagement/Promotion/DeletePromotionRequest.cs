using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Promotion
{
    public class DeletePromotionRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}