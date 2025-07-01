using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.ReviewManagement.Review;
using AppleShop.Application.Validators.ReviewManagement.Review;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ReviewManagement.Review
{
    public class GetAllReviewHandler : IRequestHandler<GetAllReviewRequest, Result<PagedList<ProductReviewDTO>>>
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IReviewMediaRepository reviewMediaRepository;
        private readonly IUserRepository userRepository;
        private readonly IReviewReplyRepository reviewReplyRepository;
        private readonly IFileService fileService;

        public GetAllReviewHandler(IReviewRepository reviewRepository, IReviewMediaRepository reviewMediaRepository, IUserRepository userRepository, IReviewReplyRepository reviewReplyRepository, IFileService fileService)
        {
            this.reviewRepository = reviewRepository;
            this.reviewMediaRepository = reviewMediaRepository;
            this.userRepository = userRepository;
            this.reviewReplyRepository = reviewReplyRepository;
            this.fileService = fileService;
        }

        public async Task<Result<PagedList<ProductReviewDTO>>> Handle(GetAllReviewRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetAllReviewValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var query = reviewRepository.FindAllWithPaging(request.paginationQuery, x => x.IsDeleted == false).ToList();
            if (request.VariantId is not null) query = query.Where(x => x.VariantId == request.VariantId).ToList();

            if (request.VariantId is not null && request.Star is not null) query = query.Where(x => x.VariantId == request.VariantId && x.Rating == request.Star).ToList();

            if (request.VariantId is not null && request.Star is not null && request.IsFilterByImage is not null)
            {
                var reviewMediaIds = reviewMediaRepository.FindAll().Select(x => x.ReviewId).Distinct().ToList();
                query = query.Where(x => reviewMediaIds.Contains(x.Id) && x.VariantId == request.VariantId && x.Rating == request.Star).ToList();
            }

            if (request.VariantId is not null && request.IsFilterByImage is not null)
            {
                var reviewMediaIds = reviewMediaRepository.FindAll().Select(x => x.ReviewId).Distinct().ToList();
                query = query.Where(x => reviewMediaIds.Contains(x.Id) && x.VariantId == request.VariantId && x.IsDeleted == false).ToList();
            }

            if (request.IsDeleted is not null) query = query.Where(x => x.IsDeleted == request.IsDeleted).ToList();

            var reviews = query;
            var totalItems = reviewRepository.FindAll(x => x.IsDeleted == false).Count();
            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
            var reviewIds = reviews.Select(r => r.Id).ToList();
            var users = userRepository.FindAll(x => userIds.Contains(x.Id))
                                      .ToDictionary(u => u.Id, u => new UserDTO
                                      {
                                          Id = u.Id,
                                          Username = u.Username,
                                          Avatar = fileService.GetFullPathFileServer(u.ImageUrl)
                                      });
            var reviewImages = reviewMediaRepository.FindAll(x => reviewIds.Contains(x.ReviewId))
                                            .GroupBy(x => x.ReviewId)
                                            .ToDictionary(g => g.Key, g => g.Select(img => new ReviewMediaDTO
                                            {
                                                ImageUrl = img.Url
                                            }).ToList());

            var reviewReplies = reviewReplyRepository.FindAll(x => reviewIds.Contains(x.ReviewId))
                                            .ToDictionary(r => r.ReviewId, r => new ReviewReplyDTO
                                            {
                                                User = users.ContainsKey(r.UserId) ? users[r.UserId] : null,
                                                ReplyMessage = r.ReplyText,
                                                CreatedAt = r.CreatedAt
                                            });

            var producReviewDtos = new ProductReviewDTO
            {
                VariantId = request.VariantId,
                Reviews = reviews.Select(r => new ReviewDTO
                {
                    ReviewId = r.Id,
                    User = users[r.UserId],
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    Images = reviewImages[r.Id].Select(img => new ReviewMediaDTO
                    {
                        ImageUrl = fileService.GetFullPathFileServer(img.ImageUrl)
                    }).ToList(),
                    Reply = reviewReplies.ContainsKey(r.Id) ? reviewReplies[r.Id] : null
                }).OrderByDescending(x => x.ReviewId).ToList()
            };

            PagedList<ProductReviewDTO> pagedResult = new
            (
                new List<ProductReviewDTO> { producReviewDtos },
                totalItems,
                request.paginationQuery.PageNumber,
                request.paginationQuery.PageSize
            );

            return Result<PagedList<ProductReviewDTO>>.Ok(pagedResult);
        }
    }
}