using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.UserManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.UserManagement
{
    public class UserAddressRepository(ApplicationDbContext context) : GenericRepository<UserAddress>(context), IUserAddressRepository
    {
    }
}