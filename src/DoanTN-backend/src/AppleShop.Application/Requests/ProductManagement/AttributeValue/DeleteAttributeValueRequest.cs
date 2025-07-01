using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.AttributeValue
{
    public class DeleteAttributeValueRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}