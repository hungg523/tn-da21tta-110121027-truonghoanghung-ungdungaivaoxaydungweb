using AppleShop.Application.Requests.DTOs.UserManagement.User;

namespace AppleShop.Application.Requests.DTOs.ReturnManagement.Return
{
    public class ReturnFullDTO
    {
        public int? ReturnId { get; set; }
        public string? OrderCode { get; set; }
        public string? Email { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}