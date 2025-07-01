using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using AppleShop.Application.Validators.ProductManagement.AttributeValue;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Infrastructure.Repositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.AttributeValue
{
    public class UpdateAttributeValueHandler : IRequestHandler<UpdateAttributeValueRequest, Result<object>>
    {
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly IMapper mapper;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly ICacheService cacheService;

        public UpdateAttributeValueHandler(IAttributeValueRepository attributeValueRepository, IAttributeRepository attributeRepository, IMapper mapper, ICacheService cacheService, IProductAttributeRepository productAttributeRepository)
        {
            this.attributeValueRepository = attributeValueRepository;
            this.attributeRepository = attributeRepository;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.productAttributeRepository = productAttributeRepository;
        }

        public async Task<Result<object>> Handle(UpdateAttributeValueRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateAttributeValueValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attributeValue = await attributeValueRepository.FindByIdAsync(request.Id, true);
            if (attributeValue is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.AttributeValue));

            var attribute = await attributeRepository.FindByIdAsync(request.AttributeId, true);
            if (attribute is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Attribute));

            var variants = productAttributeRepository.FindAll(x => x.AvId == attributeValue.Id, false).ToList();

            using var transaction = await attributeValueRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                mapper.Map(request, attributeValue);
                attributeValueRepository.Update(attributeValue!);
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