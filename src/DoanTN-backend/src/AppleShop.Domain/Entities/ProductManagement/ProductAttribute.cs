using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class ProductAttribute : BaseEntity
    {
        public int? VariantId { get; set; }
        public int? AvId { get; set; }
    }
}