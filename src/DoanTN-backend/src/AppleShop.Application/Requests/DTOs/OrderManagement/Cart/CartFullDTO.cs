namespace AppleShop.Application.Requests.DTOs.OrderManagement.Cart
{
    public class CartFullDTO
    {
        public int? Id { get; set; }
        public List<CartItemFullDTO>? CartItems { get; set; } = new List<CartItemFullDTO>();
        public decimal? TotalPrice { get; set; }
    }
}