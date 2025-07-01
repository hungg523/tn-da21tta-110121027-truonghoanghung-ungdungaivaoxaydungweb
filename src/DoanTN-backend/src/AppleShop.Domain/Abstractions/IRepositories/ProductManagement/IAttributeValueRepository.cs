using AppleShop.Domain.Abstractions.IRepositories.Base;
using AppleShop.Domain.Entities.ProductManagement;
using System.Linq.Expressions;

namespace AppleShop.Domain.Abstractions.IRepositories.ProductManagement
{
    public interface IAttributeValueRepository : IGenericRepository<AttributeValue>
    {
    }
}