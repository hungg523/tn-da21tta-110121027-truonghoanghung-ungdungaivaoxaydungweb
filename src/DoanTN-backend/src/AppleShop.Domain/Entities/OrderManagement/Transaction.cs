using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.OrderManagement
{
    public class Transaction : BaseEntity
    {
        public int? OrderId { get; set; }
        public string? PaymentGateway { get; set; }
        public string? Code { get; set; }
        public decimal? Amount { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}