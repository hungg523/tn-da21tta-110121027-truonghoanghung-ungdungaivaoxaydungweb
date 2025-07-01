using AppleShop.Application.Requests.ReviewManagement.ReviewReport;
using AppleShop.Application.Validators.ReviewManagement.ReviewReport;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ReviewManagement.ReviewReport
{
    public class HandleReportHandler : IRequestHandler<HandleReportRequest, Result<object>>
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IReviewReportRepository reviewReportRepository;
        private readonly IMapper mapper;

        public HandleReportHandler(IReviewRepository reviewRepository, IReviewReportRepository reviewReportRepository, IMapper mapper)
        {
            this.reviewRepository = reviewRepository;
            this.reviewReportRepository = reviewReportRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(HandleReportRequest request, CancellationToken cancellationToken)
        {
            var validator = new HandleReportValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var report = await reviewReportRepository.FindByIdAsync(request.ReportId, true);
            if (report is null) AppleException.ThrowNotFound(typeof(Entities.ReviewManagement.ReviewReport));

            var review = await reviewRepository.FindByIdAsync(report.ReviewId, true);
            if (review is null) AppleException.ThrowNotFound(typeof(Entities.ReviewManagement.Review));

            using var transaction = await reviewReportRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.Status == 1)
                {
                    review.IsDeleted = true;
                    reviewRepository.Update(review);
                    await reviewRepository.SaveChangesAsync(cancellationToken);
                }

                mapper.Map(request, report);
                reviewReportRepository.Update(report);
                await reviewReportRepository.SaveChangesAsync(cancellationToken);

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