using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using AppleShop.Application.Validators.ProductManagement.AttributeValue;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.AttributeValue
{
    public class GetDetailAttributeValueHandler : IRequestHandler<GetDetailAttributeValueRequest, Result<Entities.ProductManagement.AttributeValue>>
    {
        private readonly IAttributeValueRepository attributeValueRepository;

        public GetDetailAttributeValueHandler(IAttributeValueRepository attributeValueRepository)
        {
            this.attributeValueRepository = attributeValueRepository;
        }

        public async Task<Result<Entities.ProductManagement.AttributeValue>> Handle(GetDetailAttributeValueRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailAttributeValueValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attributeValue = await attributeValueRepository.FindByIdAsync(request.Id);
            if (attributeValue is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.AttributeValue));

            return Result<Entities.ProductManagement.AttributeValue>.Ok(attributeValue);
        }
    }
}