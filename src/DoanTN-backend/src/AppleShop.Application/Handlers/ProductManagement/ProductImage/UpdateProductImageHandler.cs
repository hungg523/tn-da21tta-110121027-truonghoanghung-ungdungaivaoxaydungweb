using AppleShop.Application.Requests.ProductManagement.ProductImage;
using AppleShop.Application.Validators.ProductManagement.ProductImage;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ProductManagement.ProductImage
{
    public class UpdateProductImageHandler : IRequestHandler<UpdateProductImageRequest, Result<object>>
    {
        private readonly IProductImageRepository productImageRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly IFileService fileUploadService;
        private readonly IProductVariantRepository productVariantRepository;

        public UpdateProductImageHandler(IProductImageRepository productImageRepository, IProductRepository productRepository, IMapper mapper, IFileService fileUploadService, IProductVariantRepository productVariantRepository)
        {
            this.productImageRepository = productImageRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.fileUploadService = fileUploadService;
            this.productVariantRepository = productVariantRepository;
        }

        public async Task<Result<object>> Handle(UpdateProductImageRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateProductImageValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var productImage = await productImageRepository.FindByIdAsync(request.Id, true);
            if (productImage is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductImage));

            if (request.ProductId is not null)
            {
                var product = await productRepository.FindByIdAsync(request.ProductId!, true);
                if (product is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.Product));
            }

            if (request.VariantId is not null)
            {
                var productVariant = await productVariantRepository.FindByIdAsync(request.VariantId!, true);
                if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));
            }

            using var transaction = await productImageRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                mapper.Map(request, productImage);
                productImageRepository.Update(productImage!);
                if (request.ImageData is not null)
                {
                    var imageData = request.ImageData.Split(",")[1];
                    var uploadFile = new UploadFileRequest
                    {
                        Content = imageData,
                        AssetType = AssetType.Product,
                        Suffix = productImage.Id.ToString()
                    };
                    productImage.Url = await fileUploadService.UploadFileAsync(uploadFile);
                }

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