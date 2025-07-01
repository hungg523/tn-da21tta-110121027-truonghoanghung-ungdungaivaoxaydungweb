using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ReviewManagement
{
    public class ReviewReply : BaseEntity
    {
        public int? ReviewId { get; set; }
        public int? UserId { get; set; }
        public string? ReplyText { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}