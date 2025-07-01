using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Domain.Entities.AIManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.AIManagement
{
    public class AIPromptRepository(ApplicationDbContext context) : GenericRepository<AIPrompt>(context), IAIPromptRepository
    {
    }
}