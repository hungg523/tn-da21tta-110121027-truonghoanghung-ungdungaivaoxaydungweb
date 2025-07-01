namespace AppleShop.Application.Requests.DTOs.ProductManagement.Product
{
    public class ProductAttributeFullDTO
    {
        public int? AvId { get; set; }
        public int? AttributeId { get; set; }
        public int? VariantId { get; set; }
        public string? ImageUrl { get; set; }
        public string? AttributeName { get; set; }
        public string? AttributeValue { get; set; }
        public decimal? Price { get; set; }
    }
}