using AppleShop.Domain.Abstractions.IRepositories.Base;

namespace AppleShop.Domain.Abstractions.IRepositories.ProductManagement
{
    public interface IAttributeRepository : IGenericRepository<Entities.ProductManagement.Attribute>
    {
        Task<(List<int>? existingIds, List<int>? missingIds)> CheckIdsExistAsync(List<int>? ids);
    }
}