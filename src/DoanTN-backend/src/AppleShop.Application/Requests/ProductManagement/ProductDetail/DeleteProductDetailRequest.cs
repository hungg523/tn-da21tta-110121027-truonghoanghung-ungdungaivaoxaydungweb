using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductDetail
{
    public class DeleteProductDetailRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}