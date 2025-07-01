using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ProductManagement.Attribute
{
    public class CreateAttributeRequest : ICommand
    {
        public string? Name { get; set; }
    }
}