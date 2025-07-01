using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ChatManagement.Chat
{
    public class ToggleBotRequest : ICommand
    {
        public int ConversationId { get; set; }
        public bool IsBotEnabled { get; set; }
    }
} 