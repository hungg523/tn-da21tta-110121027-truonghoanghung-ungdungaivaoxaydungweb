using AppleShop.Application.Requests.DTOs.OrderManagement.Cart;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.OrderManagement.Cart
{
    public class GetDetailCartRequest : IQuery<CartFullDTO>
    {
        public int? UserId { get; set; }
    }
}