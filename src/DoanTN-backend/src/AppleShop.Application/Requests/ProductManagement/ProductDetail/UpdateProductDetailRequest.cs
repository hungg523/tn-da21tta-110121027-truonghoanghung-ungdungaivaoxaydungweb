using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductDetail
{
    public class UpdateProductDetailRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? ProductId { get; set; }
        public string? DetailKey { get; set; }
        public string? DetailValue { get; set; }
    }
}