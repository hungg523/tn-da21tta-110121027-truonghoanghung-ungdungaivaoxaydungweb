namespace AppleShop.Application.Requests.DTOs.OrderManagement.Cart
{
    public class CartItemFullDTO
    {
        public int? VariantId { get; set; }
        public int? Quantity { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public List<string>? ProductAttributes { get; set; }
    }
}