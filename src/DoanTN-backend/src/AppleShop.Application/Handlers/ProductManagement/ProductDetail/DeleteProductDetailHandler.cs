using AppleShop.Application.Requests.ProductManagement.ProductDetail;
using AppleShop.Application.Validators.ProductManagement.ProductDetail;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Infrastructure.Repositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductDetail
{
    public class DeleteProductDetailHandler : IRequestHandler<DeleteProductDetailRequest, Result<object>>
    {
        private readonly IProductDetailRepository productDetailRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly ICacheService cacheService;

        public DeleteProductDetailHandler(IProductDetailRepository productDetailRepository, IProductVariantRepository productVariantRepository, ICacheService cacheService)
        {
            this.productDetailRepository = productDetailRepository;
            this.productVariantRepository = productVariantRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(DeleteProductDetailRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteProductDetailValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productDetail = await productDetailRepository.FindByIdAsync(request.Id, true);
            if (productDetail is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductDetail));

            var variants = productVariantRepository.FindAll(x => x.ProductId == productDetail.ProductId, false).ToList();

            using var transaction = await productDetailRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                productDetailRepository.Delete(productDetail);
                await productDetailRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();
                if (variants.Any())
                {
                    foreach (var variant in variants) await cacheService.RemoveAsync($"product_detail_{variant.Id}");
                }
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