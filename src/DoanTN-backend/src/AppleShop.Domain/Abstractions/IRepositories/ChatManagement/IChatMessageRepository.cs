using AppleShop.Domain.Abstractions.IRepositories.Base;
using AppleShop.Domain.Entities.ChatManagement;

namespace AppleShop.Domain.Abstractions.IRepositories.ChatManagement
{
    public interface IChatMessageRepository : IGenericRepository<ChatMessages>
    {
    }
}