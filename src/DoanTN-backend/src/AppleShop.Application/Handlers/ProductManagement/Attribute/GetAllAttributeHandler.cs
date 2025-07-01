using AppleShop.Application.Requests.ProductManagement.Attribute;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.Attribute
{
    public class GetAllAttributeHandler : IRequestHandler<GetAllAttributeRequest, Result<List<Entities.ProductManagement.Attribute>>>
    {
        private readonly IAttributeRepository attributeRepository;

        public GetAllAttributeHandler(IAttributeRepository attributeRepository)
        {
            this.attributeRepository = attributeRepository;
        }

        public async Task<Result<List<Entities.ProductManagement.Attribute>>> Handle(GetAllAttributeRequest request, CancellationToken cancellationToken)
        {
            var attributes = attributeRepository.FindAll().ToList();
            return Result<List<Entities.ProductManagement.Attribute>>.Ok(attributes);
        }
    }
}