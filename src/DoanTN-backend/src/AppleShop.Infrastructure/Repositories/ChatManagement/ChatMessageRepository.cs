using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Domain.Entities.ChatManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.ChatManagement
{
    public class ChatMessageRepository(ApplicationDbContext context) : GenericRepository<ChatMessages>(context), IChatMessageRepository
    {
    }
}