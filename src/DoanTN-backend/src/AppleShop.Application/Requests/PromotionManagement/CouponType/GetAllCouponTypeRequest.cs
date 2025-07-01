using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.PromotionManagement.CouponType
{
    public class GetAllCouponTypeRequest : IQuery<List<CouponTypeDTO>>
    {
    }
}