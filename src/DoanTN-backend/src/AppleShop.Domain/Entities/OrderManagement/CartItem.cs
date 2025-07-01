using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.OrderManagement
{
    public class CartItem : BaseEntity
    {
        public int? CartId { get; set; }
        public int? VariantId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public Cart Cart { get; set; }
    }
}