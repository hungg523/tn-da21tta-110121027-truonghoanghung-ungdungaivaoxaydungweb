using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ReturnManagement
{
    public class Return : BaseEntity
    {
        public int? OiId { get; set; }
        public int? UserId { get; set; }
        public string? Reason { get; set; }
        public int? Quantity { get; set; }
        public decimal? RefundAmount { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ReturnType { get; set; }
        public string? Url { get; set; }
    }
}