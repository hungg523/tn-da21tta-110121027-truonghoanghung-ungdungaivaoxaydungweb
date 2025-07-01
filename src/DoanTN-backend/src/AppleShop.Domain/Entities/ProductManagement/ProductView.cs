using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class ProductView : BaseEntity
    {
        public int? VariantId { get; set; }
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public int? UserId { get; set; }
        public int? View { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}