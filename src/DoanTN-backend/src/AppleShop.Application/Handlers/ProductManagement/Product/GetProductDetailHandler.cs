using AppleShop.Application.Requests.ProductManagement.Product;
using AppleShop.Application.Validators.ProductManagement.Product;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.Product
{
    public class GetProductDetailHandler : IRequestHandler<GetProductDetailRequest, Result<Entities.ProductManagement.Product>>
    {
        private readonly IProductRepository productRepository;

        public GetProductDetailHandler(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }

        public async Task<Result<Entities.ProductManagement.Product>> Handle(GetProductDetailRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetProductDetailValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var product = await productRepository.FindByIdAsync(request.Id);
            if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product));

            return Result<Entities.ProductManagement.Product>.Ok(product);
        }
    }
}