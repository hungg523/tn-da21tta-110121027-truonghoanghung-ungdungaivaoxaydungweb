using AppleShop.Application.Handlers.ProductManagement.ProductVariant;
using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ProductManagement.ProductVariant
{
    public class GetAllProductVariantRequest : IQuery<ProductVariantResponseDTO>
    {
        public int? IsActived { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public bool? IsDescending { get; set; }
        public bool? IsAscending { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}