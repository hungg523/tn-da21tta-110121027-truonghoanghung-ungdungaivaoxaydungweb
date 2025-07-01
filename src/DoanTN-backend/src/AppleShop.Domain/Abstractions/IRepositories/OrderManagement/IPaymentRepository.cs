using AppleShop.Domain.Abstractions.IRepositories.Base;
using AppleShop.Domain.Entities.OrderManagement;

namespace AppleShop.Domain.Abstractions.IRepositories.OrderManagement
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
    }
}