using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ProductManagement.ProductDetail
{
    public class GetAllProductDetailRequest : IQuery<List<ProductDetailDTO>>
    {
    }
}