using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class UpdateProductVariantRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? ProductId { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public int? IsActived { get; set; }
        public ICollection<int>? AvIds { get; set; }
    }
}