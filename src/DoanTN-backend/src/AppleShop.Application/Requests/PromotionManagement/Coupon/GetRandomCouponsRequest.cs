using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Coupon
{
    public class GetRandomCouponsRequest : IQuery<List<CouponDTO>>
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? Count { get; set; } = 5;
    }
} 