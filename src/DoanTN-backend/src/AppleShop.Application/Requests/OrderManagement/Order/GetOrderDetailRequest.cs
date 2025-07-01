using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.OrderManagement.Order
{
    public class GetOrderDetailRequest : IQuery<OrderFullDTO>
    {
        [JsonIgnore]
        public int? OrderId { get; set; }
    }
}