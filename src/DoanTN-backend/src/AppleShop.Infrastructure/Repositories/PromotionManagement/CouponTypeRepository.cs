using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.PromotionManagement
{
    public class CouponTypeRepository(ApplicationDbContext context) : GenericRepository<CouponType>(context), ICouponTypeRepository
    {
    }
}