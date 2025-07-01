using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.OrderManagement
{
    public class OrderItemRepository(ApplicationDbContext context) : GenericRepository<OrderItem>(context), IOrderItemRepository
    {
    }
}