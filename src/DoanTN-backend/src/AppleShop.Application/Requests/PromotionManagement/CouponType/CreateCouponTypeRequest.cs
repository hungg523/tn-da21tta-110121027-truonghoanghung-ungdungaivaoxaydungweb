using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.PromotionManagement.CouponType
{
    public class CreateCouponTypeRequest : ICommand
    {
        public int? Name { get; set; }
        public string? Description { get; set; }
    }
}