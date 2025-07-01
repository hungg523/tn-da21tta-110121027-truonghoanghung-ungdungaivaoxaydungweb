using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.PromotionManagement
{
    public class ProductPromotionRepository(ApplicationDbContext context) : GenericRepository<ProductPromotion>(context), IProductPromotionRepository
    {
    }
}