using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.AIManagement.AIPrompt
{
    public class CreatePromptRequest : ICommand
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
    }
}