using AppleShop.Domain.Abstractions.IRepositories.Base;

namespace AppleShop.Domain.Abstractions.IRepositories.UserManagement
{
    public interface IUserRepository : IGenericRepository<Entities.UserManagement.User>
    {
    }
}