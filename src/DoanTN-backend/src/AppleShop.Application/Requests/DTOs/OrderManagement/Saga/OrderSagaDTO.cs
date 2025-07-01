using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.DTOs.OrderManagement.Saga
{
    public class OrderSagaDTO
    {
        [JsonIgnore]
        public string? Code { get; set; }

        [JsonIgnore]
        public decimal? Amount { get; set; }
        public string? PaymentUrl { get; set; }
    }
}