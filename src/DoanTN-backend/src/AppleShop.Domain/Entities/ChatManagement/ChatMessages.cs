using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ChatManagement
{
    public class ChatMessages : BaseEntity
    {
        public int? ConversationId { get; set; }
        public string? SenderType { get; set; }
        public int? SenderId { get; set; }
        public string? Message { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool? IsFromBot { get; set; }
    }
}