using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class GetSimilarProductsRequest : IQuery<List<ProductFullDTO>>
    {
        [JsonIgnore]
        public int? VariantId { get; set; }
        public int? Limit { get; set; } = 5;
    }
} 