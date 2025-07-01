using AppleShop.Application.Requests.ProductManagement.Product;
using AppleShop.Application.Validators.ProductManagement.Product;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.Product
{
    public class CreateProductHandler : IRequestHandler<CreateProductRequest, Result<object>>
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly ICategoryRepository categoryRepository;

        public CreateProductHandler(IProductRepository productRepository,
                                   IMapper mapper,
                                   ICategoryRepository categoryRepository)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.categoryRepository = categoryRepository;
        }

        public async Task<Result<object>> Handle(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateProductValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            if (request.CategoryId is not null)
            {
                var category = await categoryRepository.FindByIdAsync(request.CategoryId);
                if (category is null) AppleException.ThrowNotFound(message: "Category is not found.");
            }

            var product = mapper.Map<Entities.ProductManagement.Product>(request);

            using var transaction = await productRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                productRepository.Create(product);
                await productRepository.SaveChangesAsync(cancellationToken);
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