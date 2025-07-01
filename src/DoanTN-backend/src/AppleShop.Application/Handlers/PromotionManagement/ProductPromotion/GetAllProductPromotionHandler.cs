using AppleShop.Application.Requests.DTOs.PromotionManagement.ProductPromotion;
using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.PromotionManagement.ProductPromotion
{
    public class GetAllProductPromotionHandler : IRequestHandler<GetAllProductPromotionRequest, Result<List<ProductPromotionDTO>>>
    {
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;

        public GetAllProductPromotionHandler(IProductPromotionRepository productPromotionRepository,
                                            IProductRepository productRepository, IProductVariantRepository productVariantRepository,
                                            IProductAttributeRepository productAttributeRepository,
                                            IAttributeValueRepository attributeValueRepository)
        {
            this.productPromotionRepository = productPromotionRepository;
            this.productRepository = productRepository;
            this.productVariantRepository = productVariantRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
        }

        public async Task<Result<List<ProductPromotionDTO>>> Handle(GetAllProductPromotionRequest request, CancellationToken cancellationToken)
        {
            var productPromotions = productPromotionRepository.FindAll().ToList();

            var variants = productVariantRepository.FindAll(x => productPromotions.Select(x => x.VariantId).Contains(x.Id)).ToDictionary(p => p.Id);
            var products = productRepository.FindAll(x => productPromotions.Select(p => p.VariantId).Contains(x.Id)).ToDictionary(p => p.Id);

            var productAttributes = productAttributeRepository.FindAll(x => productPromotions.Select(x => x.VariantId).Contains(x.VariantId)).ToList();
            var attributeValues = attributeValueRepository.FindAll().ToDictionary(av => av.Id, av => av.Value);

            var productPms = productRepository.FindAll(x => productPromotions.Select(p => p.ProductId).Contains(x.Id)).ToDictionary(p => p.Id);

            var productPromotionDtos = productPromotions.Select(pm => new ProductPromotionDTO
            {
                ProductId = pm.ProductId,
                ProductName = productPms.ContainsKey(pm.ProductId ?? 0) ? productPms[pm.ProductId].Name : null,
                VariantId = pm.VariantId,
                VariantName = variants.ContainsKey(pm.VariantId ?? 0) ?
                $"{products[variants[pm.VariantId].ProductId].Name} - {string.Join(" ", productAttributes.Where(pa => pa.VariantId == pm.VariantId).Select(pa => attributeValues.TryGetValue(pa.AvId, out var value) ? value : "Unknown"))}" 
                : null
            }).ToList();

            return Result<List<ProductPromotionDTO>>.Ok(productPromotionDtos);
        }
    }
}