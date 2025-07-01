using AppleShop.Application.Requests.DTOs.UserManagement.User;

namespace AppleShop.Application.Requests.DTOs.OrderManagement.Order
{
    public class OrderItemUserDTO
    {
        public int? OiId { get; set; }
        public int? VariantId { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? ProductAttribute { get; set; }
        public int? Quantity { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public UserAddressDTO? UserAddresses { get; set; }
    }
}