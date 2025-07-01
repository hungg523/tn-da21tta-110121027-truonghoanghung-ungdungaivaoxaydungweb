using AppleShop.Application.Requests.ProductManagement.Attribute;
using AppleShop.Application.Validators.ProductManagement.Attribute;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.Attribute
{
    public class GetDetailAttributeHandler : IRequestHandler<GetDetailAttributeRequest, Result<Entities.ProductManagement.Attribute>>
    {
        private readonly IAttributeRepository attributeRepository;

        public GetDetailAttributeHandler(IAttributeRepository attributeRepository)
        {
            this.attributeRepository = attributeRepository;
        }

        public async Task<Result<Entities.ProductManagement.Attribute>> Handle(GetDetailAttributeRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailAttributeValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attribute = await attributeRepository.FindByIdAsync(request.Id);
            if (attribute is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Attribute));

            return Result<Entities.ProductManagement.Attribute>.Ok(attribute);
        }
    }
}