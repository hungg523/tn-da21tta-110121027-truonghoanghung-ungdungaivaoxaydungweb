using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.PromotionManagement
{
    public class CouponType : BaseEntity
    {
        public int? Name { get; set; }
        public string? Description { get; set; }
    }
}