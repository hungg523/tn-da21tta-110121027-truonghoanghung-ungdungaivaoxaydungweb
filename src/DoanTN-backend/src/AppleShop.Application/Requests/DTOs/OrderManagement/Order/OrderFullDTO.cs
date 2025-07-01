namespace AppleShop.Application.Requests.DTOs.OrderManagement.Order
{
    public class OrderFullDTO
    {
        public int? OrderId { get; set; }
        public string? Code { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public ICollection<OrderItemUserDTO>? Items { get; set; } = new List<OrderItemUserDTO>();
    }
}