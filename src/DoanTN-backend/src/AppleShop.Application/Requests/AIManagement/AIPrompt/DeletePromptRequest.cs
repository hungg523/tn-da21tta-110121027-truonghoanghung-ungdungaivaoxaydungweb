using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.AIManagement.AIPrompt
{
    public class DeletePromptRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}