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
    public class UpdateAttributeHandler : IRequestHandler<UpdateAttributeRequest, Result<object>>
    {
        private readonly IAttributeRepository attributeRepository;
        private readonly IMapper mapper;

        public UpdateAttributeHandler(IAttributeRepository attributeRepository, IMapper mapper)
        {
            this.attributeRepository = attributeRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(UpdateAttributeRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateAttributeValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var attribute = await attributeRepository.FindByIdAsync(request.Id, true);
            if (attribute is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Attribute));

            using var transaction = await attributeRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                mapper.Map(request, attribute);
                attributeRepository.Update(attribute!);
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