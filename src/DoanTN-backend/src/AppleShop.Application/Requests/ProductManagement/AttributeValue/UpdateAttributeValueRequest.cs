using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.AttributeValue
{
    public class UpdateAttributeValueRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? AttributeId { get; set; }
        public string? Value { get; set; }
    }
}