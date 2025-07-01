using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.OrderManagement
{
    public class Payment : BaseEntity
    {
        public int? OrderId { get; set; }
        public int? PaymentMethod { get; set; }
        public decimal? Amount { get; set; }
        public int? Status { get; set; }
        public string? TransactionCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}