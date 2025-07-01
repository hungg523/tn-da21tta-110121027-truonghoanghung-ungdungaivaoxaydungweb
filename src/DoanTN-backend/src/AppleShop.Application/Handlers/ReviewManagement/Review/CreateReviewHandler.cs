using AppleShop.Application.Requests.ReviewManagement.Review;
using AppleShop.Application.Validators.ReviewManagement.Review;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ReviewManagement.Review
{
    public class CreateReviewHandler : IRequestHandler<CreateReviewRequest, Result<object>>
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;
        private readonly IMapper mapper;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IOrderItemRepository orderItemRepository;

        public CreateReviewHandler(IReviewRepository reviewRepository, IUserRepository userRepository, IFileService fileService, IMapper mapper, IProductVariantRepository productVariantRepository, IOrderItemRepository orderItemRepository)
        {
            this.reviewRepository = reviewRepository;
            this.userRepository = userRepository;
            this.fileService = fileService;
            this.mapper = mapper;
            this.productVariantRepository = productVariantRepository;
            this.orderItemRepository = orderItemRepository;
        }

        public async Task<Result<object>> Handle(CreateReviewRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateReviewValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var user = await userRepository.FindByIdAsync(request.UserId);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            var productVariant = await productVariantRepository.FindByIdAsync(request.VariantId);
            if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

            var orderItem = await orderItemRepository.FindByIdAsync(request.OiId, true);
            if (orderItem is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.OrderItem));

            var review = mapper.Map<Entities.ReviewManagement.Review>(request);

            using var transaction = await reviewRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                orderItem.IsReview = true;
                orderItemRepository.Update(orderItem);
                reviewRepository.Create(review);
                await orderItemRepository.SaveChangesAsync(cancellationToken);
                await reviewRepository.SaveChangesAsync(cancellationToken);
                if (request.ImageDatas is not null)
                {
                    var mediaTasks = request.ImageDatas?.Select(async (image, index) =>
                    {
                        var uploadFile = new UploadFileRequest
                        {
                            Content = image,
                            AssetType = AssetType.Review,
                            Suffix = $"{request.UserId}_{Guid.NewGuid().ToString().Substring(0,6).ToLower()}_{index + 1}"
                        };

                        return new Entities.ReviewManagement.ReviewMedia
                        {
                            ReviewId = review.Id,
                            Url = await fileService.UploadFileAsync(uploadFile)
                        };

                    }).ToList();

                    review.Media = (await Task.WhenAll(mediaTasks!)).ToList();
                }

                await reviewRepository.SaveChangesAsync(cancellationToken);
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