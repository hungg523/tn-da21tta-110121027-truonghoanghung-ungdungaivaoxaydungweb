using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Domain.Entities.CategoryManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.CategoryManagement
{
    public class CategoryRepository(ApplicationDbContext context) : GenericRepository<Category>(context), ICategoryRepository
    {
    }
}