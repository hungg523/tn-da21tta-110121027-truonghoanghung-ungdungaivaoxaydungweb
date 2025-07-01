using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.OrderManagement
{
    public class OrderItem : BaseEntity
    {
        public int? OrderId { get; set; }
        public int? VariantId { get; set; }
        public int? Quantity { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? ItemStatus { get; set; }
        public bool? IsReview { get; set; }
    }
}