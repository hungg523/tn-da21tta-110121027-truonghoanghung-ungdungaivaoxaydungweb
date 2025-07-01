using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class SearchProductRequest : IQuery<ProductVariantResponseDTO>
    {
        public string? Name { get; set; }
        
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}