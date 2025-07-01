using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using AppleShop.Application.Validators.ProductManagement.ProductVariant;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductVariant
{
    public class DeleteProductVariantHandler : IRequestHandler<DeleteProductVariantRequest, Result<object>>
    {
        private readonly IProductVariantRepository productVariantRepository;
        private readonly ICacheService cacheService;

        public DeleteProductVariantHandler(IProductVariantRepository productVariantRepository, ICacheService cacheService)
        {
            this.productVariantRepository = productVariantRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(DeleteProductVariantRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteProductVariantValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productVariant = await productVariantRepository.FindByIdAsync(request.Id, true);
            if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

            using var transaction = await productVariantRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                productVariant!.IsActived = 0;
                productVariantRepository.Update(productVariant);
                await productVariantRepository.SaveChangesAsync(cancellationToken);

                transaction.Commit();

                await cacheService.RemoveByPatternAsync("product_variants_*");

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