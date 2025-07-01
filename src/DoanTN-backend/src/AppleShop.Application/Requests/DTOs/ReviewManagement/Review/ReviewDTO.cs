using AppleShop.Application.Requests.DTOs.UserManagement.User;

namespace AppleShop.Application.Requests.DTOs.ReviewManagement.Review
{
    public class ReviewDTO
    {
        public int? ReviewId { get; set; }
        public UserDTO? User { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<ReviewMediaDTO>? Images { get; set; } = new List<ReviewMediaDTO>();
        public ReviewReplyDTO? Reply { get; set; }
    }
}