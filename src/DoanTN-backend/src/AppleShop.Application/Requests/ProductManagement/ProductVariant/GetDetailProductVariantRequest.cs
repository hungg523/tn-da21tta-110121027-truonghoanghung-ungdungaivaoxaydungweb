using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class GetDetailProductVariantRequest : IQuery<ProductFullDTO>
    {
        [JsonIgnore]
        public int? VariantId { get; set; }

        [JsonIgnore]
        public int? UserId { get; set; }
    }
}