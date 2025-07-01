using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.OrderManagement
{
    public class CartItemRepository(ApplicationDbContext context) : GenericRepository<CartItem>(context), ICartItemRepository
    {
    }
}