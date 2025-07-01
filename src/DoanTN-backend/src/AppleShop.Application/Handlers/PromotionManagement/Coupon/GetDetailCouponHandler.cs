using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Application.Requests.PromotionManagement.Coupon;
using AppleShop.Application.Validators.PromotionManagement.Coupon;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using AppleEnum = AppleShop.Share.Enumerations;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Coupon
{
    public class GetDetailCouponHandler : IRequestHandler<GetDetailCouponRequest, Result<CouponDTO>>
    {
        private readonly ICouponRepository couponRepository;
        private readonly ICouponTypeRepository couponTypeRepository;

        public GetDetailCouponHandler(ICouponRepository couponRepository, ICouponTypeRepository couponTypeRepository)
        {
            this.couponRepository = couponRepository;
            this.couponTypeRepository = couponTypeRepository;
        }

        public async Task<Result<CouponDTO>> Handle(GetDetailCouponRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailCouponValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var coupon = await couponRepository.FindSingleAsync(x => x.Code.ToUpper() == request.Code.ToUpper(), false);
            if (coupon is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.Coupon));

            var couponType = await couponTypeRepository.FindByIdAsync(coupon.CtId);
            if (couponType.Name.ToString().ToLower() == AppleEnum.CouponType.SHIP.ToString().ToLower()) AppleException.ThrowNotFound(message: "Invalid Coupon");

            var couponDto = new CouponDTO
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Description = coupon.Description,
                DiscountPercentage = coupon.DiscountPercentage,
                DiscountAmount = coupon.DiscountAmount,
                MinOrderValue = coupon.MinOrderValue,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                TimesUsed = coupon.TimesUsed,
                MaxUsage = coupon.MaxUsage,
                MaxUsagePerUser = coupon.MaxUsagePerUser,
                CouponType = ((AppleEnum.CouponType)couponType.Name).ToString(),
                AvailableDate = $"{(coupon.EndDate - coupon.StartDate).Value.Days} ngày",
                DiscountDisplay = coupon.DiscountPercentage > 0
                ? $"Giảm {coupon.DiscountPercentage}% cho đơn từ {coupon.MinOrderValue} VND (Tối đa {coupon.MaxDiscountAmount} VND)"
                : $"Giảm {coupon.DiscountAmount} VND cho đơn từ {coupon.MinOrderValue} VND"
            };

            return Result<CouponDTO>.Ok(couponDto);
        }
    }
}