using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Domain.Entities.EventManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.EventManagement
{
    public class SpinHistoryRepository(ApplicationDbContext context) : GenericRepository<SpinHistory>(context), ISpinHistoryRepository
    {
    }
}