using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.EventManagement
{
    public class SpinHistory : BaseEntity
    {
        public int? UserId { get; set; }
        public int? CouponId { get; set; }
        public DateOnly? SpinDate { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}