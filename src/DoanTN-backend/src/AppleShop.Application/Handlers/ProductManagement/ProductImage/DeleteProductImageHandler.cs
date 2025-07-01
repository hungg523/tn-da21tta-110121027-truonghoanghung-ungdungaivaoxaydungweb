using AppleShop.Application.Requests.ProductManagement.ProductImage;
using AppleShop.Application.Validators.ProductManagement.ProductImage;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductImage
{
    public class DeleteProductImageHandler : IRequestHandler<DeleteProductImageRequest, Result<object>>
    {
        private readonly IProductImageRepository productImageRepository;
        private readonly IMapper mapper;

        public DeleteProductImageHandler(IProductImageRepository productImageRepository, IMapper mapper)
        {
            this.productImageRepository = productImageRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(DeleteProductImageRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteProductImageValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            using var transaction = await productImageRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var productImage = await productImageRepository.FindByIdAsync(request.Id, true);
                if (productImage is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductImage));

                productImageRepository.Delete(productImage!);
                await productImageRepository.SaveChangesAsync(cancellationToken);
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