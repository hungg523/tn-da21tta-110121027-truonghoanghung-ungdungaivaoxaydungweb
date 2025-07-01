using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class CreateProductVariantRequest : ICommand
    {
        public int? ProductId { get; set; }
        public decimal? Price { get; set; } = 0;
        public int? Stock { get; set; } = 0;

        [JsonIgnore]
        public int? ReserveStock { get; set; } = 0;

        [JsonIgnore]
        public int? SoldQuantity { get; set; } = 0;
        public int? IsActived { get; set; } = 0;
        public ICollection<int>? AvIds { get; set; }
    }
}