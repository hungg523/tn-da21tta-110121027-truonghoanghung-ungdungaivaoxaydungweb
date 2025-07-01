using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ChatManagement.Chat
{
    public class GetPendingConversationsRequest : IQuery<List<ConversationDTO>>
    {
    }
} 