using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class GetColorOptionsRequest : IQuery<List<ProductAttributeFullDTO>>
    {
        public int? VariantId { get; set; }
    }
}