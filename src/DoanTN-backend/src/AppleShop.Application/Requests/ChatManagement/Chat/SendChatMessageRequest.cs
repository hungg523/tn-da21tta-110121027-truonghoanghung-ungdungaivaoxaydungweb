using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ChatManagement.Chat
{
    public class SendChatMessageRequest : ICommand<ChatMessageDTO>
    {
        [JsonIgnore]
        public int? ConversationId { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }

        [JsonIgnore]
        public int? UserId { get; set; }
    }
} 