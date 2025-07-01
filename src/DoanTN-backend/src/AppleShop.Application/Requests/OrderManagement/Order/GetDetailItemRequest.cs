using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.OrderManagement.Order
{
    public class GetDetailItemRequest : IQuery<OrderItemUserDTO>
    {
        public int? OiId { get; set; }
    }
}