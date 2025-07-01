using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class AttributeValue : BaseEntity
    {
        public int? AttributeId { get; set; }
        public string? Value { get; set; }
    }
}