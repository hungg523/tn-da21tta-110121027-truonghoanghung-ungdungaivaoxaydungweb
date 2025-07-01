using AppleShop.Application.Requests.DTOs.UserManagement.User;

namespace AppleShop.Application.Requests.DTOs.ReviewManagement.Review
{
    public class ReviewReplyDTO
    {
        public UserDTO? User { get; set; }
        public string? ReplyMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}