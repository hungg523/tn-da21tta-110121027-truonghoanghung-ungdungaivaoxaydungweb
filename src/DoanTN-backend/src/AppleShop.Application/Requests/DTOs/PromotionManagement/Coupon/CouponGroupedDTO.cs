namespace AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon
{
    public class CouponGroupedDTO
    {
        public List<CouponDTO>? CouponOrder { get; set; }
        public List<CouponDTO>? CouponShip { get; set; }
    }
}