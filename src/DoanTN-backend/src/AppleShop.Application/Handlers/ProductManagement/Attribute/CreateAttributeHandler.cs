using AppleShop.Application.Requests.ProductManagement.Attribute;
using AppleShop.Application.Validators.ProductManagement.Attribute;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.Attribute
{
    public class CreateAttributeHandler : IRequestHandler<CreateAttributeRequest, Result<object>>
    {
        private readonly IAttributeRepository attributeRepository;
        private readonly IMapper mapper;

        public CreateAttributeHandler(IAttributeRepository attributeRepository, IMapper mapper)
        {
            this.attributeRepository = attributeRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(CreateAttributeRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateAttributeValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attribute = mapper.Map<Entities.ProductManagement.Attribute>(request);
            using var transaction = await attributeRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                attributeRepository.Create(attribute);
                await attributeRepository.SaveChangesAsync(cancellationToken);
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