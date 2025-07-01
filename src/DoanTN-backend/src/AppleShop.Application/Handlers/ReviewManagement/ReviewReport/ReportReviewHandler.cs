using AppleShop.Application.Requests.ReviewManagement.ReviewReport;
using AppleShop.Application.Services;
using AppleShop.Application.Validators.ReviewManagement.ReviewReport;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using AppleShop.Infrastructure.Repositories.ProductManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Org.BouncyCastle.Asn1.Cms;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ReviewManagement.ReviewReport
{
    public class ReportReviewHandler : IRequestHandler<ReportReviewRequest, Result<object>>
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IReviewReportRepository reviewReportRepository;
        private readonly IReviewReportUserRepository reviewReportUserRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly IProductRepository productRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IAttributeRepository attributeRepository;

        public ReportReviewHandler(IReviewRepository reviewRepository,
            IReviewReportRepository reviewReportRepository,
            IReviewReportUserRepository reviewReportUserRepository,
            IUserRepository userRepository,
            IMapper mapper,
            INotificationService notificationService,
            IProductRepository productRepository,
            IAttributeValueRepository attributeValueRepository,
            IProductAttributeRepository productAttributeRepository,
            IProductVariantRepository productVariantRepository,
            IAttributeRepository attributeRepository)
        {
            this.reviewRepository = reviewRepository;
            this.reviewReportRepository = reviewReportRepository;
            this.reviewReportUserRepository = reviewReportUserRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.productRepository = productRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.productVariantRepository = productVariantRepository;
            this.attributeRepository = attributeRepository;
        }

        public async Task<Result<object>> Handle(ReportReviewRequest request, CancellationToken cancellationToken)
        {
            var validator = new ReportReviewValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var review = await reviewRepository.FindByIdAsync(request.ReviewId, true);
            if (review is null) AppleException.ThrowNotFound(typeof(Entities.ReviewManagement.Review));

            var user = await userRepository.FindByIdAsync(request.UserId, false);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            var userHasReport = await reviewReportUserRepository.FindSingleAsync(x => x.UserId == request.UserId, true);
            if (userHasReport is not null)
            {
                var report = await reviewReportRepository.FindSingleAsync(x => userHasReport.ReportId == x.Id, true);
                if (report is not null && report.ReviewId == request.ReviewId) AppleException.ThrowConflict("You have reported this comment, please wait for a response from the administrator.");
            }

            var variant = await productVariantRepository.FindByIdAsync(review.VariantId);
            var product = await productRepository.FindByIdAsync(variant.ProductId);

            var productAttributes = productAttributeRepository.FindAll(x => x.VariantId == variant.Id).ToList();
            var attributeValues = attributeValueRepository.FindAll().ToDictionary(av => av.Id, av => av);
            var attributes = attributeRepository.FindAll(x => attributeValues.Select(x => x.Value.AttributeId).Contains(x.Id)).ToDictionary(a => a.Id, a => a);

            var fullAttributes = (from pa in productAttributes
                                  where pa.VariantId == variant.Id
                                  join av in attributeValues.Values on pa.AvId equals av.Id
                                  join attr in attributes.Values on av.AttributeId equals attr.Id
                                  select new
                                  {
                                      AttributeName = attr.Name,
                                      AttributeValue = av.Value
                                  }).ToList();

            string[] desiredOrder = { "kích thước", "màu sắc", "ram", "dung lượng", "cổng sạc" };

            var orderedAttributes = fullAttributes
                                    .OrderBy(attr =>
                                    {
                                        var lowerName = attr.AttributeName.ToLower();
                                        for (int i = 0; i < desiredOrder.Length; i++)
                                        {
                                            if (lowerName.Contains(desiredOrder[i])) return i;
                                        }
                                        return desiredOrder.Length;
                                    }).ToList();

            var baseName = product.Name;
            var variantName = baseName + " " + string.Join(" ", orderedAttributes.Select(a => a.AttributeValue));

            using var transaction = await reviewReportRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var existingReport = await reviewReportRepository.FindSingleAsync(x => x.ReviewId == request.ReviewId, true);
                if (existingReport is not null)
                {
                    existingReport.TotalReports += 1;
                    reviewReportRepository.Update(existingReport);
                }
                else
                {
                    existingReport = mapper.Map<Entities.ReviewManagement.ReviewReport>(request);
                    review.IsFlagged = true;

                    reviewReportRepository.Create(existingReport);
                    reviewRepository.Update(review);
                }

                await reviewReportRepository.SaveChangesAsync(cancellationToken);
                await reviewRepository.SaveChangesAsync(cancellationToken);

                var userReport = mapper.Map<ReviewReportUser>(request);
                userReport.ReportId = existingReport.Id;
                reviewReportUserRepository.Create(userReport);
                await reviewReportUserRepository.SaveChangesAsync(cancellationToken);
                await notificationService.NotifyNewReportedComment(review.Id.Value, variantName.Trim());
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