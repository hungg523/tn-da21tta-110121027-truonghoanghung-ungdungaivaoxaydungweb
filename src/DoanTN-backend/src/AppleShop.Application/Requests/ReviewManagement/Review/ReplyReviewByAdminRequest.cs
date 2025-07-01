using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReviewManagement.Review
{
    public class ReplyReviewByAdminRequest : ICommand
    {
        public int? ReviewId { get; set; }

        [JsonIgnore]
        public int? UserId { get; set; }
        public string? ReplyText { get; set; }

        [JsonIgnore]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}