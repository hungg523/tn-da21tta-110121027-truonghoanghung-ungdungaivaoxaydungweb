using AppleShop.Application.Requests.ProductManagement.ProductDetail;
using AppleShop.Application.Validators.ProductManagement.ProductDetail;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Infrastructure.Repositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductDetail
{
    public class UpdateProductDetailHandler : IRequestHandler<UpdateProductDetailRequest, Result<object>>
    {
        private readonly IProductDetailRepository productDetailRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly ICacheService cacheService;

        public UpdateProductDetailHandler(IProductDetailRepository productDetailRepository, IProductRepository productRepository, IMapper mapper, IProductVariantRepository productVariantRepository, ICacheService cacheService)
        {
            this.productDetailRepository = productDetailRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.productVariantRepository = productVariantRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(UpdateProductDetailRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateProductDetailValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productDetail = await productDetailRepository.FindByIdAsync(request.Id, true);
            if (productDetail is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductDetail));

            if (request.ProductId is not null)
            {
                var product = await productRepository.FindByIdAsync(request.ProductId, true);
                if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product));
            }

            var variants = productVariantRepository.FindAll(x => x.ProductId == productDetail.ProductId, false).ToList();

            mapper.Map(request, productDetail);

            using var transaction = await productDetailRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                productDetailRepository.Update(productDetail);
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