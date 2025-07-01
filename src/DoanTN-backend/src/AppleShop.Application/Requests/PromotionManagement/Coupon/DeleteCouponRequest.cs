using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Coupon
{
    public class DeleteCouponRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}