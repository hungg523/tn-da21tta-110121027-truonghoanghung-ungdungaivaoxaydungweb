using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ChatManagement
{
    public class Conversations : BaseEntity
    {
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public bool? IsBotHandled { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}