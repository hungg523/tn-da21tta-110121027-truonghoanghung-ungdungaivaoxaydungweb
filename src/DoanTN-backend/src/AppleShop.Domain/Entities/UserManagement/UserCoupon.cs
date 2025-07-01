using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.UserManagement
{
    public class UserCoupon : BaseEntity
    {
        public int? UserId { get; set; }
        public int? CouponId { get; set; }
        public bool? IsUsed { get; set; }
        public int? TimesUsed { get; set; }
        public DateTime? ClaimedAt { get; set; }
    }
}