using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.UserManagement
{
    public class AuthRepository(ApplicationDbContext context) : GenericRepository<Domain.Entities.UserManagement.Auth>(context), IAuthRepository
    {
    }
}