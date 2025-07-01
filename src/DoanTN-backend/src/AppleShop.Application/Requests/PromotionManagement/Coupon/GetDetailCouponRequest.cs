using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Coupon
{
    public class GetDetailCouponRequest : IQuery<CouponDTO>
    {
        [JsonIgnore]
        public string? Code { get; set; }
    }
}