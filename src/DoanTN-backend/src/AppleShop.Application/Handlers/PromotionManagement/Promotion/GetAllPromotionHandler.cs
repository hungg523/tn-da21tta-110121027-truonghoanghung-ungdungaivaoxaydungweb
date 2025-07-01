using Entities = AppleShop.Domain.Entities;
using AppleShop.Share.Shared;
using MediatR;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Application.Requests.PromotionManagement.Promotion;
using AppleShop.Application.Requests.DTOs.PromotionManagement.ProductPromotion;
using AppleShop.Infrastructure.Repositories.ProductManagement;
using AppleShop.Infrastructure.Repositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;

namespace AppleShop.Application.Handlers.PromotionManagement.Promotion
{
    public class GetAllPromotionHandler : IRequestHandler<GetAllPromotionRequest, Result<List<PromotionDTO>>>
    {
        private readonly IPromotionRepository promotionRepository;
        private readonly IProductPromotionRepository productPromotionRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;

        public GetAllPromotionHandler(IPromotionRepository promotionRepository,
                                    IProductPromotionRepository productPromotionRepository,
                                    IProductRepository productRepository,
                                    IProductVariantRepository productVariantRepository,
                                    IProductAttributeRepository productAttributeRepository,
                                    IAttributeValueRepository attributeValueRepository)
        {
            this.promotionRepository = promotionRepository;
            this.productPromotionRepository = productPromotionRepository;
            this.productRepository = productRepository;
            this.productVariantRepository = productVariantRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
        }

        public async Task<Result<List<PromotionDTO>>> Handle(GetAllPromotionRequest request, CancellationToken cancellationToken)
        {
            List<Entities.PromotionManagement.Promotion> promotions;
            if (request.IsActived is null) promotions = promotionRepository.FindAll(includeProperties: pm => pm.ProductPromotions).ToList();
            else promotions = promotionRepository.FindAll(x => x.IsActived == request.IsActived, false, pm => pm.ProductPromotions).ToList();

            var productPromotions = productPromotionRepository.FindAll(x => promotions.Select(x => x.Id).Contains(x.PromotionId)).ToList();

            var variants = productVariantRepository.FindAll(x => productPromotions.Select(x => x.VariantId).Contains(x.Id)).ToDictionary(p => p.Id);
            var products = productRepository.FindAll(x => variants.Select(p => p.Value.ProductId).Contains(x.Id)).ToDictionary(p => p.Id);

            var productAttributes = productAttributeRepository.FindAll(x => productPromotions.Select(x => x.VariantId).Contains(x.VariantId)).ToList();
            var attributeValues = attributeValueRepository.FindAll().ToDictionary(av => av.Id, av => av.Value);

            var productPms = productRepository.FindAll(x => productPromotions.Select(p => p.ProductId).Contains(x.Id)).ToDictionary(p => p.Id);

            var promotionDtos = promotions.Select(p => new PromotionDTO
            {
                PromotionId = p.Id,
                Name = p.Name,
                Description = p.Description,
                DiscountPercentage = p.DiscountPercentage,
                DiscountAmount = p.DiscountAmout,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActived = p.IsActived,
                IsFlashSale = p.IsFlashSale,
                Promotions = p.ProductPromotions.Select(pm => new ProductPromotionDTO
                {
                    PmId = pm.Id,
                    VariantId = pm.VariantId,
                    VariantName = (pm.VariantId.HasValue && variants.ContainsKey(pm.VariantId.Value)) ?
                                    $"{(
                                        products.ContainsKey(variants[pm.VariantId.Value].ProductId)
                                        ? products[variants[pm.VariantId.Value].ProductId].Name
                                        : "Unknown Product"
                                    )} - {string.Join(" ", productAttributes
                                            .Where(pa => pa.VariantId == pm.VariantId)
                                            .Select(pa => attributeValues.TryGetValue(pa.AvId, out var value) ? value : "Unknown"))}"
                                    : null,
                }).ToList()
            }).ToList();

            return Result<List<PromotionDTO>>.Ok(promotionDtos);
        }
    }
}