using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using AppleShop.Application.Validators.ProductManagement.AttributeValue;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.AttributeValue
{
    public class DeleteAttributeValueHandler : IRequestHandler<DeleteAttributeValueRequest, Result<object>>
    {
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly ICacheService cacheService;

        public DeleteAttributeValueHandler(IAttributeValueRepository attributeValueRepository, IProductAttributeRepository productAttributeRepository, ICacheService cacheService)
        {
            this.attributeValueRepository = attributeValueRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(DeleteAttributeValueRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteAttributeValueValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attributeValue = await attributeValueRepository.FindByIdAsync(request.Id, true);
            if (attributeValue is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.AttributeValue));

            var productAttribute = productAttributeRepository.FindByIdAsync(attributeValue.Id, false);
            if (productAttribute is not null) AppleException.ThrowConflict("Attribute value has been used.");

            var variants = productAttributeRepository.FindAll(x => x.AvId == attributeValue.Id, false).ToList();

            using var transaction = await attributeValueRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                attributeValueRepository.Delete(attributeValue!);
                await attributeValueRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();
                if (variants.Any())
                {
                    await cacheService.RemoveByPatternAsync("product_variants_*");
                    foreach (var variant in variants) await cacheService.RemoveAsync($"product_detail_{variant.VariantId}");
                }
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