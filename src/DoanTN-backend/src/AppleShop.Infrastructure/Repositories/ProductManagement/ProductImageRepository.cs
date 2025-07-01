using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.ProductManagement
{
    public class ProductImageRepository(ApplicationDbContext context) : GenericRepository<ProductImage>(context), IProductImageRepository
    {
        public void CreateRange(IEnumerable<ProductImage> productImages)
        {
            context.AddRange(productImages);
        }

    }
}