using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Application.Requests.ProductManagement.ProductDetail;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ProductManagement.ProductDetail
{
    public class GetAllProductDetailHandler : IRequestHandler<GetAllProductDetailRequest, Result<List<ProductDetailDTO>>>
    {
        private readonly IProductDetailRepository productDetailRepository;
        private readonly IProductRepository productRepository;

        public GetAllProductDetailHandler(IProductDetailRepository productDetailRepository, IProductRepository productRepository)
        {
            this.productDetailRepository = productDetailRepository;
            this.productRepository = productRepository;
        }

        public async Task<Result<List<ProductDetailDTO>>> Handle(GetAllProductDetailRequest request, CancellationToken cancellationToken)
        {
            var productDetails = productDetailRepository.FindAll().ToList();
            var productIds = productDetails.Select(d => d.ProductId).Distinct().ToList();
            var products = productRepository.FindAll(x => productIds.Contains(x.Id)).ToList();

            var groupedDetails = productDetails.GroupBy(d => d.ProductId)
                .Select(group => new ProductDetailDTO
                {
                    ProductId = group.Key,
                    ProductName = products.FirstOrDefault(p => p.Id == group.Key)?.Name ?? "Unknown",
                    Detail = group.Select(detail => new DetailDTO
                    {
                        Key = detail.DetailKey,
                        Value = detail.DetailValue
                    }).ToList()
                }).ToList();

            return Result<List<ProductDetailDTO>>.Ok(groupedDetails);
        }
    }
}