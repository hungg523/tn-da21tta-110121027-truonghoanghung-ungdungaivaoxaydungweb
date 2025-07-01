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
    public class CreateProductDetailHandler : IRequestHandler<CreateProductDetailRequest, Result<object>>
    {
        private readonly IProductDetailRepository productDetailRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly ICacheService cacheService;

        public CreateProductDetailHandler(IProductDetailRepository productDetailRepository, IProductRepository productRepository, IMapper mapper, IProductVariantRepository productVariantRepository, ICacheService cacheService)
        {
            this.productDetailRepository = productDetailRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.productVariantRepository = productVariantRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateProductDetailRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateProductDetailValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var product = await productRepository.FindByIdAsync(request.ProductId, true);
            if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product));

            var variants = productVariantRepository.FindAll(x => x.ProductId == product.Id, false).ToList();

            var productDetail = mapper.Map<Entities.ProductManagement.ProductDetail>(request);

            using var transaction = await productDetailRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                productDetailRepository.Create(productDetail);
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