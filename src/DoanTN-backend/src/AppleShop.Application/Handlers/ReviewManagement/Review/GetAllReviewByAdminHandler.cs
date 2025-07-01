using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.ReviewManagement.Review;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ReviewManagement.Review
{
    public class GetAllReviewByAdminHandler : IRequestHandler<GetAllReviewByAdminRequest, Result<List<ProductReviewDTO>>>
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IReviewMediaRepository reviewMediaRepository;
        private readonly IUserRepository userRepository;
        private readonly IReviewReplyRepository reviewReplyRepository;
        private readonly IFileService fileService;

        public GetAllReviewByAdminHandler(IReviewRepository reviewRepository,
                                        IReviewMediaRepository reviewMediaRepository,
                                        IUserRepository userRepository,
                                        IReviewReplyRepository reviewReplyRepository,
                                        IFileService fileService)
        {
            this.reviewRepository = reviewRepository;
            this.reviewMediaRepository = reviewMediaRepository;
            this.userRepository = userRepository;
            this.reviewReplyRepository = reviewReplyRepository;
            this.fileService = fileService;
        }

        public async Task<Result<List<ProductReviewDTO>>> Handle(GetAllReviewByAdminRequest request, CancellationToken cancellationToken)
        {
            var query = reviewRepository.FindAll(x => x.IsDeleted == false).ToList();
            if (request.VariantId is not null) query = query.Where(x => x.VariantId == request.VariantId).ToList();

            if (request.VariantId is not null && request.Star is not null) query = query.Where(x => x.VariantId == request.VariantId && x.Rating == request.Star).ToList();

            var reviews = query;
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

            return Result<List<ProductReviewDTO>>.Ok(new List<ProductReviewDTO> { producReviewDtos });
        }
    }
}