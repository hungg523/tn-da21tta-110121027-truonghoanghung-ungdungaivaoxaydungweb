using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ChatManagement.Chat
{
    public class GetConversationRequest : IQuery<ConversationDTO>
    {
        public int? UserId { get; set; }
        public string? Status { get; set; }
    }
}