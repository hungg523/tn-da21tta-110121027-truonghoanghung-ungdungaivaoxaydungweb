using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using AppleShop.Application.Validators.ProductManagement.ProductVariant;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductVariant
{
    public class CreateProductVariantHandler : IRequestHandler<CreateProductVariantRequest, Result<object>>
    {
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly ICacheService cacheService;

        public CreateProductVariantHandler(IProductVariantRepository productVariantRepository, IProductRepository productRepository, IMapper mapper, IAttributeValueRepository attributeValueRepository, ICacheService cacheService)
        {
            this.productVariantRepository = productVariantRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.attributeValueRepository = attributeValueRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateProductVariantRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateProductVariantValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var product = await productRepository.FindByIdAsync(request.ProductId, true);
            if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

            var productVariant = new Entities.ProductManagement.ProductVariant 
            {
                ProductId = request.ProductId,
                Price = request.Price,
                Stock = request.Stock,
                ReservedStock = 0,
                SoldQuantity = 0,
                IsActived = request.IsActived,
            };

            using var transaction = await productVariantRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.AvIds is not null)
                {
                    var attributeValues = await attributeValueRepository.FindByIds(request.AvIds.ToList(), false, cancellationToken);
                    var invalidAvIds = request.AvIds.Except(attributeValues.Select(d => d.Id.Value)).ToList();
                    if (invalidAvIds.Any()) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.AttributeValue), string.Join("Attribute Value has not found by Id ", invalidAvIds));

                    var duplicateAvIds = request.AvIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                    if (duplicateAvIds.Any()) AppleException.ThrowConflict(string.Join("Duplication Attribute Value Id found with Id ", duplicateAvIds));

                    productVariant.ProductAttributes = request.AvIds.Select(avId => new Entities.ProductManagement.ProductAttribute
                    {
                        VariantId = productVariant.Id,
                        AvId = avId,
                    }).ToList();
                }

                productVariantRepository.Create(productVariant);
                await productVariantRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.RemoveByPatternAsync("product_variants_*");

                return Result<object>.Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}