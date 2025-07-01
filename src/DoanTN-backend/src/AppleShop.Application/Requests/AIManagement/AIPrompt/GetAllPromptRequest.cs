using AppleShop.Share.Abstractions;
using Entities = AppleShop.Domain.Entities.EventManagement;

namespace AppleShop.Application.Requests.AIManagement.AIPrompt
{
    public class GetAllPromptRequest : IQuery<List<Domain.Entities.AIManagement.AIPrompt>>
    {
    }
}