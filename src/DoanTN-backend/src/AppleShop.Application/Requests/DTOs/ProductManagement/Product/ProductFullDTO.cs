using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.DTOs.ProductManagement.Product
{
    public class ProductFullDTO
    {
        public int? VariantId { get; set; }
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? Stock { get; set; }
        public int? ReservedStock { get; set; }
        public int? ActualStock { get; set; }
        public int? SoldQuantity { get; set; }
        public int? TotalReviews { get; set; }
        public double? AverageRating { get; set; }
        public int? IsActived { get; set; }
        public ICollection<ProductImageDTO>? Images { get; set; } = new List<ProductImageDTO>();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<ProductAttributeFullDTO>? ProductsAttributes { get; set; } = new List<ProductAttributeFullDTO>();
        public ICollection<DetailDTO>? Details { get; set; } = new List<DetailDTO>();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? FlashSaleStartDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? FlashSaleEndDate { get; set; }
    }
}