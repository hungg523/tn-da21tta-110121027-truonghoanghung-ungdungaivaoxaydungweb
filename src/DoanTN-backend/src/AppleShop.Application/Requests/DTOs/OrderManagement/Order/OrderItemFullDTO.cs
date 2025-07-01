using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.DTOs.OrderManagement.Order
{
    public class OrderItemFullDTO
    {
        public int? OiId { get; set; }
        public int? VariantId { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? ProductAttribute { get; set; }
        public int? Quantity { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? TotalPrice { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PaymentUrl { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsReview { get; set; }
    }
}