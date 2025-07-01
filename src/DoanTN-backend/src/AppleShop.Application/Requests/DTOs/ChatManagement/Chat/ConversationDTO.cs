namespace AppleShop.Application.Requests.DTOs.ChatManagement.Chat
{
    public class ConversationDTO
    {
        public int? ConversationId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Avartar { get; set; }
        public string? Status { get; set; }
        public bool? IsBotHandled { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<ChatMessageDTO>? Messages { get; set; }
    }
} 