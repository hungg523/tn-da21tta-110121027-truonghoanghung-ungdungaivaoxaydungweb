using AppleShop.Application.Requests.DTOs.UserManagement.User;

namespace AppleShop.Application.Requests.DTOs.ReturnManagement.Return
{
    public class ReturnDetailDTO
    {
        public int? ReturnId { get; set; }
        public int? VariantId { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public string? ReturnUrl { get; set; }
        public string? ProductAttribute { get; set; }
        public int? Quantity { get; set; }
        public decimal? RefundAmount { get; set; }
        public UserAddressDTO? UserAddresses { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ReturnType { get; set; }
    }
}