using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class ProductDetail : BaseEntity
    {
        public int? ProductId { get; set; }
        public string? DetailKey { get; set; }
        public string? DetailValue { get; set; }
    }
}