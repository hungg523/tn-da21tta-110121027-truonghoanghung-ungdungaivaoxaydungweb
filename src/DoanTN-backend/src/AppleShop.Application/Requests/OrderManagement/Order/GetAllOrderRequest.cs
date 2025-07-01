using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.OrderManagement.Order
{
    public class GetAllOrderRequest : IQuery<OrderListResponseDTO>
    {
        public int? UserId { get; set; }
        public int? Status { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}