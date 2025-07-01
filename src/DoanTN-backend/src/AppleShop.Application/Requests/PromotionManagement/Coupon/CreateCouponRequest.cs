using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.Coupon
{
    public class CreateCouponRequest : ICommand
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? DiscountPercentage { get; set; } = 0;
        public decimal? DiscountAmount { get; set; } = 0;
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderValue { get; set; } = 0;

        [JsonIgnore]
        public int? TimesUsed { get; set; } = 0;
        public int? MaxUsage { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public bool? IsVip { get; set; } = false;
        public bool? UserSpecific { get; set; } = false;
        public string? Terms { get; set; }
        public int? CtId { get; set; }
        public bool? IsActived { get; set; } = true;
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;

        [JsonIgnore]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}