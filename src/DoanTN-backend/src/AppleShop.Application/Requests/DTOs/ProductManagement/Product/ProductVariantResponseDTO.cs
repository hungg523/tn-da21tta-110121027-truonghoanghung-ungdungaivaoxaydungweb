using AppleShop.Application.Requests.DTOs.ProductManagement.Product;

namespace AppleShop.Application.Requests.DTOs.ProductManagement.Product
{
    public class ProductVariantResponseDTO
    {
        public int TotalItems { get; set; }
        public List<ProductFullDTO> ProductVariants { get; set; } = new List<ProductFullDTO>();
    }
}