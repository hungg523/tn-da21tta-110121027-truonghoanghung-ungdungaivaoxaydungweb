using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;
using AppleShop.Application.Requests.DTOs.ReviewManagement.ReviewReport;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.ReviewManagement.ReviewReport;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ReviewManagement.ReviewReport
{
    public class GetAllReportHandler : IRequestHandler<GetAllReportRequest, Result<List<ReviewReportDTO>>>
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IReviewMediaRepository reviewMediaRepository;
        private readonly IReviewReportRepository reviewReportRepository;
        private readonly IReviewReportUserRepository reviewReportUserRepository;
        private readonly IUserRepository userRepository;

        public GetAllReportHandler(
            IReviewRepository reviewRepository,
            IReviewReportRepository reviewReportRepository,
            IReviewReportUserRepository reviewReportUserRepository,
            IUserRepository userRepository,
            IReviewMediaRepository reviewMediaRepository)
        {
            this.reviewRepository = reviewRepository;
            this.reviewReportRepository = reviewReportRepository;
            this.reviewReportUserRepository = reviewReportUserRepository;
            this.userRepository = userRepository;
            this.reviewMediaRepository = reviewMediaRepository;
        }

        public async Task<Result<List<ReviewReportDTO>>> Handle(GetAllReportRequest request, CancellationToken cancellationToken)
        {
            var reports = reviewReportRepository.FindAll().ToList();

            var reviewIds = reports.Select(r => r.ReviewId).Distinct().ToList();
            var reviews = reviewRepository.FindAll(x => reviewIds.Contains(x.Id)).ToDictionary(r => r.Id, r => r);

            var reviewImages = reviewMediaRepository.FindAll(x => reviewIds.Contains(x.ReviewId))
                .GroupBy(x => x.ReviewId)
                .ToDictionary(g => g.Key, g => g.Select(img => new ReviewMediaDTO
                {
                    ImageUrl = img.Url
                }).ToList());

            var userIds = reports
                .SelectMany(r => reviewReportUserRepository.FindAll(x => x.ReportId == r.Id).Select(u => u.UserId))
                .Distinct()
                .ToList();
            var users = userRepository.FindAll(x => userIds.Contains(x.Id))
                .ToDictionary(u => u.Id, u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Avatar = u.ImageUrl
                });

            var reportDetails = reviewReportUserRepository.FindAll(x => reports.Select(r => r.Id).Contains(x.ReportId)).ToList();
            var reviewReportsDto = reports.Select(report =>
            {
                var review = reviews[report.ReviewId];

                return new ReviewReportDTO
                {
                    Id = report.Id.Value,
                    Review = new ReviewDTO
                    {
                        ReviewId = review.Id,
                        User = users.ContainsKey(review.UserId) ? users[review.UserId] : null,
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreatedAt = review.CreatedAt,
                        Images = reviewImages.ContainsKey(review.Id) ? reviewImages[review.Id] : new List<ReviewMediaDTO>()
                    },
                    Status = ((ReportStatus)report.Status).ToString(),
                    TotalReports = report.TotalReports,
                    ReportUsers = reportDetails.Where(x => x.ReportId == report.Id)
                        .Select(reportUser => new ReportUserDTO
                        {
                            User = users.ContainsKey(reportUser.UserId) ? users[reportUser.UserId] : null,
                            Reason = reportUser.Reason,
                            CreatedAt = reportUser.CreatedAt
                        }).ToList()
                };
            }).OrderByDescending(x => x.Id).ToList();

            return Result<List<ReviewReportDTO>>.Ok(reviewReportsDto);
        }
    }
}