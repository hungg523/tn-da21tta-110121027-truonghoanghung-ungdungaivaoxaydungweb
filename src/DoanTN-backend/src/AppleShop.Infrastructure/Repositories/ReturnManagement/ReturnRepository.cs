using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Domain.Entities.ReturnManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.ReturnManagement
{
    public class ReturnRepository(ApplicationDbContext context) : GenericRepository<Return>(context), IReturnRepository
    {
    }
}