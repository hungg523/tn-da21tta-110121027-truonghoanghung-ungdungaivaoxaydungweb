using AppleShop.Application.Requests.PromotionManagement.Coupon;
using AppleShop.Application.Validators.PromotionManagement.Coupon;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Coupon
{
    public class DeleteCouponHandler : IRequestHandler<DeleteCouponRequest, Result<object>>
    {
        private readonly ICouponRepository couponRepository;
        private readonly IMapper mapper;

        public DeleteCouponHandler(ICouponRepository couponRepository, IMapper mapper)
        {
            this.couponRepository = couponRepository;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(DeleteCouponRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteCouponValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var coupon = await couponRepository.FindByIdAsync(request.Id, true);
            if (coupon is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Coupon));

            using var transaction = await couponRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                couponRepository.Delete(coupon!);
                await couponRepository.SaveChangesAsync(cancellationToken);

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