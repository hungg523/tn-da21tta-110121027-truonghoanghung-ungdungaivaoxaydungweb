using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.ProductManagement
{
    public class ProductDetailRepository(ApplicationDbContext context) : GenericRepository<ProductDetail>(context), IProductDetailRepository
    {
    }
}