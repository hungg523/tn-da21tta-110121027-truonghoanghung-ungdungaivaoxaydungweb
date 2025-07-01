using AppleShop.Application.Requests.ProductManagement.Product;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.Product
{
    public class GetAllProductHandler : IRequestHandler<GetAllProductRequest, Result<List<Entities.ProductManagement.Product>>>
    {
        private readonly IProductRepository productRepository;
        private readonly ICacheService cacheService;

        public GetAllProductHandler(IProductRepository productRepository, ICacheService cacheService)
        {
            this.productRepository = productRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<Entities.ProductManagement.Product>>> Handle(GetAllProductRequest request, CancellationToken cancellationToken)
        {
            var products = productRepository.FindAll().OrderBy(x => x.Name).ToList();
            var cacheKey = $"products";
            var cachedResult = await cacheService.GetAsync<Result<List<Entities.ProductManagement.Product>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var result = Result<List<Entities.ProductManagement.Product>>.Ok(products);
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
    }
}