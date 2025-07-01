namespace AppleShop.Application.Requests.DTOs.ChatManagement.Chat
{
    public class ChatMessageDTO
    {
        public int? ChatId { get; set; }
        public int? ConversationId { get; set; }
        public string? SenderType { get; set; }
        public int? SenderId { get; set; }
        public string? Message { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool? IsFromBot { get; set; }
    }
} 