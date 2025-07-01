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
    public class UpdateProductVariantHandler : IRequestHandler<UpdateProductVariantRequest, Result<object>>
    {
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly ICacheService cacheService;

        public UpdateProductVariantHandler(IProductVariantRepository productVariantRepository, IProductRepository productRepository, IMapper mapper, IProductAttributeRepository productAttributeRepository, IAttributeValueRepository attributeValueRepository, ICacheService cacheService)
        {
            this.productVariantRepository = productVariantRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(UpdateProductVariantRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateProductVariantValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productVariant = await productVariantRepository.FindByIdAsync(request.Id, true);
            if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Coupon));

            var product = await productRepository.FindByIdAsync(request.ProductId, true);
            //if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

            using var transaction = await productVariantRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.AvIds is null) { }
                else if (!request.AvIds.Any())
                {
                    var productAttributes = productAttributeRepository.FindAll(x => x.VariantId == request.Id).ToList();
                    if (productAttributes is not null) productAttributeRepository.RemoveMultiple(productAttributes);
                    await productVariantRepository.SaveChangesAsync(cancellationToken);
                }

                if (request.AvIds is not null)
                {
                    var productAttributes = productAttributeRepository.FindAll(x => x.VariantId == request.Id).ToList();
                    if (productAttributes is not null) productAttributeRepository.RemoveMultiple(productAttributes);

                    var attributeValues = await attributeValueRepository.FindByIds(request.AvIds, false, cancellationToken);
                    var invalidAvIds = request.AvIds.Except(attributeValues.Select(d => d.Id.Value)).ToList();
                    if (invalidAvIds.Any()) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.AttributeValue), string.Join("Attribute Value has not found by Id ", invalidAvIds));

                    var duplicateAvIds = request.AvIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                    if (duplicateAvIds.Any()) AppleException.ThrowConflict(string.Join("Duplication Attribute Value Id found with Id ", duplicateAvIds));

                    var newProductAttributes = request.AvIds.Select(avId => new Entities.ProductManagement.ProductAttribute
                    {
                        VariantId = productVariant.Id,
                        AvId = avId,
                    }).ToList();

                    productAttributeRepository.AddRange(newProductAttributes);
                    await attributeValueRepository.SaveChangesAsync(cancellationToken);
                }

                mapper.Map(request, productVariant);
                productVariantRepository.Update(productVariant);
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