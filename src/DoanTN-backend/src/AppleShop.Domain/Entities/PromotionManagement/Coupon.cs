using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.PromotionManagement
{
    public class Coupon : BaseEntity
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public int? TimesUsed { get; set; }
        public int? MaxUsage { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public bool? IsVip { get; set; }
        public bool? UserSpecific { get; set; }
        public string? Terms { get; set; }
        public int? CtId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActived { get; set; }
    }
}