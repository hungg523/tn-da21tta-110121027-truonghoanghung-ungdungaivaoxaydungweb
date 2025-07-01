using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.ProductManagement
{
    public class ProductViewRepository(ApplicationDbContext context) : GenericRepository<ProductView>(context), IProductViewRepository
    {
    }
}