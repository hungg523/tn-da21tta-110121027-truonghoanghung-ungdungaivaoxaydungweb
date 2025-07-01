using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ChatManagement.Chat
{
    public class AdminReplyRequest : ICommand<ChatMessageDTO>
    {
        public int? ConversationId { get; set; }
        public string? Message { get; set; }

        [JsonIgnore]
        public int? AdminId { get; set; }
    }
} 