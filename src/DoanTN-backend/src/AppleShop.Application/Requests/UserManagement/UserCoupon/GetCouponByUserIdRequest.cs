using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.UserCoupon
{
    public class GetCouponByUserIdRequest : IQuery<CouponGroupedDTO>
    {
        public int? UserId { get; set; }
    }
}