using Entities = AppleShop.Domain.Entities;
using AppleShop.Infrastructure.Repositories.Base;
using AppleShop.Share.Exceptions;
using Microsoft.EntityFrameworkCore;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;

namespace AppleShop.Infrastructure.Repositories.ProductManagement
{
    public class AttributeRepository(ApplicationDbContext context) : GenericRepository<Entities.ProductManagement.Attribute>(context), IAttributeRepository
    {
        public async Task<(List<int>? existingIds, List<int>? missingIds)> CheckIdsExistAsync(List<int>? ids)
        {
            ids = ids.Distinct().ToList() ?? new List<int>();
            var existingIds = await context.Set<Entities.ProductManagement.Attribute>()
                                   .Where(attribute => attribute.Id.HasValue && ids.Contains(attribute.Id.Value))
                                   .Select(attribute => attribute.Id.Value)
                                   .ToListAsync();
            var missingIds = ids.Except(existingIds).ToList();
            if (missingIds.Any()) AppleException.ThrowNotFound(message: $"Id {string.Join(", ", missingIds)} is not found in Attribute");
            return (existingIds, missingIds);
        }
    }
}