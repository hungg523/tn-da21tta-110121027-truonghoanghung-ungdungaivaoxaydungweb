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
    public class DeleteAttributeHandler : IRequestHandler<DeleteAttributeRequest, Result<object>>
    {
        private readonly IAttributeRepository attributeRepository;
        private readonly IMapper mapper;
        private readonly IAttributeValueRepository attributeValueRepository;

        public DeleteAttributeHandler(IAttributeRepository attributeRepository, IMapper mapper, IAttributeValueRepository attributeValueRepository)
        {
            this.attributeRepository = attributeRepository;
            this.mapper = mapper;
            this.attributeValueRepository = attributeValueRepository;
        }

        public async Task<Result<object>> Handle(DeleteAttributeRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteAttributeValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attribute = await attributeRepository.FindByIdAsync(request.Id, true);
            if (attribute is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Attribute));

            var attributeValue = await attributeValueRepository.FindSingleAsync(x => x.AttributeId == request.Id);
            if (attributeValue is not null) AppleException.ThrowConflict("Attribute has foreign key.");

            using var transaction = await attributeRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                attributeRepository.Delete(attribute!);
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