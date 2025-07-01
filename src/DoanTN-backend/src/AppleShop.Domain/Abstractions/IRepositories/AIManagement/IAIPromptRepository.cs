using AppleShop.Domain.Abstractions.IRepositories.Base;
using AppleShop.Domain.Entities.AIManagement;

namespace AppleShop.Domain.Abstractions.IRepositories.AIManagement
{
    public interface IAIPromptRepository : IGenericRepository<AIPrompt>
    {
    }
}