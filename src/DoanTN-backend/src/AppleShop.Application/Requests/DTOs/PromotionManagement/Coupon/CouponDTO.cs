namespace AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon
{
    public class CouponDTO
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? TimesUsed { get; set; }
        public int? MaxUsage { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public bool? UserSpecific { get; set; }
        public bool? IsVip { get; set; }
        public string? CouponType { get; set; }
        public string? AvailableDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? DiscountDisplay { get; set; }
        public string? Term { get; set; }
        public bool? IsAvtived { get; set; }
    }
}