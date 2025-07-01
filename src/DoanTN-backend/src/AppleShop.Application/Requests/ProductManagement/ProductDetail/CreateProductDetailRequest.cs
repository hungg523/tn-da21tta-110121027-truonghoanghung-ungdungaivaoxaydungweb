using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ProductManagement.ProductDetail
{
    public class CreateProductDetailRequest : ICommand
    {
        public int? ProductId { get; set; }
        public string? DetailKey { get; set; }
        public string? DetailValue { get; set; }
    }
}