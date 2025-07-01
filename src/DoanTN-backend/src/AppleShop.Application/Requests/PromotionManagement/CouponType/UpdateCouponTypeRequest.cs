using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.CouponType
{
    public class UpdateCouponTypeRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? Name { get; set; }
        public string? Description { get; set; }
    }
}