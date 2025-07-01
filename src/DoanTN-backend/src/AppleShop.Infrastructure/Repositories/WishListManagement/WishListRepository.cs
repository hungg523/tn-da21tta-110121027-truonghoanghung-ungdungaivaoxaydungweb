using AppleShop.Domain.Abstractions.IRepositories.WishListManagement;
using AppleShop.Domain.Entities.WishListManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.WishListManagement
{
    public class WishListRepository(ApplicationDbContext context) : GenericRepository<WishList>(context), IWishListRepository
    {
    }
}