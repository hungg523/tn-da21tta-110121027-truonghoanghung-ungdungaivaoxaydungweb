using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Domain.Entities.ChatManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.ChatManagement
{
    public class ConversationRepository(ApplicationDbContext context) : GenericRepository<Conversations>(context), IConversationRepository
    {
    }
}