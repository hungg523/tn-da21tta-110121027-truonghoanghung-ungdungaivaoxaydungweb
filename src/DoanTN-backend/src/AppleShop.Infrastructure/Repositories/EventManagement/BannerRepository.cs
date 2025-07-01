using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Domain.Entities.EventManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.EventManagement
{
    public class BannerRepository(ApplicationDbContext context) : GenericRepository<Banner>(context), IBannerRepository
    {
    }
}