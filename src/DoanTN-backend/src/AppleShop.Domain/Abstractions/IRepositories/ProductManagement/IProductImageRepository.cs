using AppleShop.Domain.Abstractions.IRepositories.Base;
using AppleShop.Domain.Entities.ProductManagement;

namespace AppleShop.Domain.Abstractions.IRepositories.ProductManagement
{
    public interface IProductImageRepository : IGenericRepository<ProductImage>
    {
        public void CreateRange(IEnumerable<ProductImage> productImages);
    }
}