using AppleShop.Application.Requests.ProductManagement.Product;
using AppleShop.Application.Validators.ProductManagement.Product;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.Product
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductRequest, Result<object>>
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IAttributeRepository colorRepository;
        private readonly IProductAttributeRepository productColorRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly ICacheService cacheService;

        public UpdateProductHandler(IProductRepository productRepository,
                                    IMapper mapper,
                                    IAttributeRepository colorRepository,
                                    IProductAttributeRepository productColorRepository,
                                    ICategoryRepository categoryRepository,
                                    IProductVariantRepository productVariantRepository,
                                    ICacheService cacheService)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.colorRepository = colorRepository;
            this.productColorRepository = productColorRepository;
            this.categoryRepository = categoryRepository;
            this.productVariantRepository = productVariantRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateProductValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var product = await productRepository.FindByIdAsync(request.Id, true);
            if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product));

            var variants = productVariantRepository.FindAll(x => x.ProductId == product.Id, false).ToList();

            if (request.CategoryId is not null)
            {
                var category = await categoryRepository.FindByIdAsync(request.CategoryId, true);
                if (category is null) AppleException.ThrowNotFound(typeof(Entities.CategoryManagement.Category));
            }

            using var transaction = await productRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                mapper.Map(request, product);

                productRepository.Update(product);
                await productRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();
                
                if (variants.Any())
                {
                    await cacheService.RemoveByPatternAsync("product_variants_*");
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