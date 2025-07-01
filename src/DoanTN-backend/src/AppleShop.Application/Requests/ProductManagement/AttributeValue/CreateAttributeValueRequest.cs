using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ProductManagement.AttributeValue
{
    public class CreateAttributeValueRequest : ICommand
    {
        public int? AttributeId { get; set; }
        public string? Value { get; set; }
    }
}