using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using AppleShop.Application.Validators.ProductManagement.AttributeValue;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.AttributeValue
{
    public class CreateAttributeValueHandler : IRequestHandler<CreateAttributeValueRequest, Result<object>>
    {
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly IMapper mapper;

        public CreateAttributeValueHandler(IAttributeValueRepository attributeValueRepository, IAttributeRepository attributeRepository, IMapper mapper)
        {
            this.attributeValueRepository = attributeValueRepository;
            this.attributeRepository = attributeRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(CreateAttributeValueRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateAttributeValueValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attribute = await attributeRepository.FindByIdAsync(request.AttributeId, true);
            if (attribute is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Attribute));

            var attributeValue = mapper.Map<Entities.ProductManagement.AttributeValue>(request);
            using var transaction = await attributeValueRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                attributeValueRepository.Create(attributeValue);
                await attributeValueRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();
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