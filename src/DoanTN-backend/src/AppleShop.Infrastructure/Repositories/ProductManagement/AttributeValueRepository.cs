using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AppleShop.Infrastructure.Repositories.ProductManagement
{
    public class AttributeValueRepository(ApplicationDbContext context) : GenericRepository<AttributeValue>(context), IAttributeValueRepository
    {
    }
}