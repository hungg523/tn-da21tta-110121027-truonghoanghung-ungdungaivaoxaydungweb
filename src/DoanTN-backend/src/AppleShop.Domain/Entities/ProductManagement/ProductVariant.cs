using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class ProductVariant : BaseEntity
    {
        public int? ProductId { get; set; }
        public decimal? Price { get; set; }
        public int? SoldQuantity { get; set; }
        public int? Stock { get; set; }
        public int? ReservedStock { get; set; }
        public int? IsActived { get; set; }

        [JsonIgnore]
        public ICollection<ProductAttribute>? ProductAttributes { get; set; }
    }
}