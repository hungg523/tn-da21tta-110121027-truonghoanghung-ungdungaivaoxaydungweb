using Entities = AppleShop.Domain.Entities;
using AppleShop.Share.Abstractions;
using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Coupon
{
    public class GetAllCouponRequest : IQuery<CouponGroupedDTO>
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public bool? IsActived { get; set; }
    }
}