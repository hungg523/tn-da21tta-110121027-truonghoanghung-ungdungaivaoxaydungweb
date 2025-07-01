using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.DTOs.OrderManagement.Order
{
    public class OrderDTO
    {
        public int? OrderId { get; set; }
        public string? Code { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? TotalAmount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PaymentUrl { get; set; }
    }
}