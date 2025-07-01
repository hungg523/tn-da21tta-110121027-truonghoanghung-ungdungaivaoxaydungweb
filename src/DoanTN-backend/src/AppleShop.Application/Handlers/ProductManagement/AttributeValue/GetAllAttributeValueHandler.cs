using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.AttributeValue
{
    public class GetAllAttributeValueHandler : IRequestHandler<GetAllAttributeValueRequest, Result<List<Entities.ProductManagement.AttributeValue>>>
    {
        private readonly IAttributeValueRepository attributeValueRepository;

        public GetAllAttributeValueHandler(IAttributeValueRepository attributeValueRepository)
        {
            this.attributeValueRepository = attributeValueRepository;
        }

        public async Task<Result<List<Entities.ProductManagement.AttributeValue>>> Handle(GetAllAttributeValueRequest request, CancellationToken cancellationToken)
        {
            var attributeValue = attributeValueRepository.FindAll().OrderBy(x => x.Value).ToList();
            return Result<List<Entities.ProductManagement.AttributeValue>>.Ok(attributeValue);
        }
    }
}