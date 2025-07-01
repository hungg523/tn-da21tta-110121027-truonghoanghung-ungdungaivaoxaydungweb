using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class ProductImage : BaseEntity
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public int? Position { get; set; }
        public int? ProductId { get; set; }
        public int? VariantId { get; set; }
    }
}