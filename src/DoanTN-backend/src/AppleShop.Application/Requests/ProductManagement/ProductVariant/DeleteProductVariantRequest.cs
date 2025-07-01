using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class DeleteProductVariantRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}