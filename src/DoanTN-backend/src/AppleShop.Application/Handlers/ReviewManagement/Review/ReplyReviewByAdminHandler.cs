using AppleShop.Application.Requests.ReviewManagement.Review;
using AppleShop.Application.Validators.ReviewManagement.Review;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ReviewManagement.Review
{
    public class ReplyReviewByAdminHandler : IRequestHandler<ReplyReviewByAdminRequest, Result<object>>
    {
        private readonly IReviewReplyRepository replyReviewRepository;
        private readonly IReviewRepository reviewRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public ReplyReviewByAdminHandler(IReviewReplyRepository replyReviewRepository, IReviewRepository reviewRepository, IUserRepository userRepository, IMapper mapper)
        {
            this.replyReviewRepository = replyReviewRepository;
            this.reviewRepository = reviewRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(ReplyReviewByAdminRequest request, CancellationToken cancellationToken)
        {
            var validator = new ReplyReviewByAdminValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var review = await reviewRepository.FindByIdAsync(request.ReviewId);
            if (review is null) AppleException.ThrowNotFound(typeof(Entities.ReviewManagement.Review));

            var admin = await userRepository.FindSingleAsync(x => x.Id == request.UserId && x.Role != 0);
            if (admin is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            var replyReview = mapper.Map<ReviewReply>(request);

            using var transaction = await replyReviewRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                replyReviewRepository.Create(replyReview);
                await replyReviewRepository.SaveChangesAsync(cancellationToken);
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