using AppleShop.Domain.Abstractions.IRepositories.Base;

namespace AppleShop.Domain.Abstractions.IRepositories.UserManagement
{
    public interface IAuthRepository : IGenericRepository<Entities.UserManagement.Auth>
    {
    }
}