using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Application.Requests.DTOs.OrderManagement.Saga;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.OrderManagement.Order
{
    public class CreateOrderRequest : ICommand<OrderSagaDTO>
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? UserAddressId { get; set; }
        public int? CouponId { get; set; }
        public int? ShipCouponId { get; set; }
        public string? Code { get; set; }
        public string? Payment { get; set; }
        public List<OrderItemDTO>? OrderItems { get; set; } = new List<OrderItemDTO>();
    }
}