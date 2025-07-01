using AppleShop.Application.Requests.DTOs.UserManagement.User;

namespace AppleShop.Application.Requests.DTOs.ReviewManagement.ReviewReport
{
    public class ReportUserDTO
    {
        public UserDTO? User { get; set; }
        public string? Reason { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}