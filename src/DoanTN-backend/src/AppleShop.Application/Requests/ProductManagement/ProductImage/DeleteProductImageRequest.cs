using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ProductManagement.ProductImage
{
    public class DeleteProductImageRequest : ICommand
    {
        public int? Id { get; set; }
    }
}