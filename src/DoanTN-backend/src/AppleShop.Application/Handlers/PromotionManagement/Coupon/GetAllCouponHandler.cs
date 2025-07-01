using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Application.Requests.PromotionManagement.Coupon;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Shared;
using MediatR;
using AppleEnum = AppleShop.Share.Enumerations;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.Coupon
{
    public class GetAllCouponHandler : IRequestHandler<GetAllCouponRequest, Result<CouponGroupedDTO>>
    {
        private readonly ICouponRepository couponRepository;
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IUserRepository userRepository;
        private readonly IUserCouponRepository userCouponRepository;

        public GetAllCouponHandler(ICouponRepository couponRepository, ICouponTypeRepository couponTypeRepository, IUserRepository userRepository, IUserCouponRepository userCouponRepository)
        {
            this.couponRepository = couponRepository;
            this.couponTypeRepository = couponTypeRepository;
            this.userRepository = userRepository;
            this.userCouponRepository = userCouponRepository;
        }

        public async Task<Result<CouponGroupedDTO>> Handle(GetAllCouponRequest request, CancellationToken cancellationToken)
        {
            var user = await userRepository.FindByIdAsync(request.UserId, false);
            List<Entities.PromotionManagement.Coupon> coupons;
            if (request.IsActived is null) coupons = couponRepository.FindAll().ToList();
            else coupons = couponRepository.FindAll(x => x.IsActived == request.IsActived).ToList();

            var expiredCoupons = couponRepository.FindAll(x => x.EndDate < DateTime.Now || x.TimesUsed == x.MaxUsage).ToList();
            if (expiredCoupons.Any())
            {
                expiredCoupons.ForEach(x => x.IsActived = false);
                couponRepository.UpdateRange(expiredCoupons);
                await couponRepository.SaveChangesAsync();
            }

            var couponTypeIds = coupons.Select(c => c.CtId).Distinct().ToList();
            var couponTypes = couponTypeRepository.FindAll(x => couponTypeIds.Contains(x.Id)).ToDictionary(ct => ct.Id, ct => ct.Name);

            var couponOrder = new List<CouponDTO>();
            var couponShip = new List<CouponDTO>();

            foreach (var coupon in coupons)
            {
                var couponDTO = new CouponDTO
                {
                    Id = coupon.Id,
                    Code = coupon.Code,
                    Description = coupon.Description,
                    DiscountPercentage = coupon.DiscountPercentage,
                    DiscountAmount = coupon.DiscountAmount,
                    MinOrderValue = coupon.MinOrderValue,
                    MaxDiscountAmount = coupon.MaxDiscountAmount,
                    UserSpecific = coupon.UserSpecific,
                    TimesUsed = coupon.TimesUsed,
                    MaxUsage = coupon.MaxUsage,
                    MaxUsagePerUser = coupon.MaxUsagePerUser,
                    CouponType = ((AppleEnum.CouponType)couponTypes[coupon.CtId]).ToString(),
                    AvailableDate = $"{(coupon.EndDate - coupon.StartDate).Value.Days} ngày",
                    DiscountDisplay = coupon.DiscountPercentage > 0
                        ? $"Giảm {coupon.DiscountPercentage}% cho đơn từ {coupon.MinOrderValue} VND (Tối đa {coupon.MaxDiscountAmount} VND)"
                        : $"Giảm {coupon.DiscountAmount} VND cho đơn từ {coupon.MinOrderValue} VND",
                    StartDate = coupon.StartDate,
                    EndDate = coupon.EndDate,
                    CreatedAt = coupon.CreatedAt,
                    Term = coupon.Terms,
                    IsVip = coupon.IsVip,
                    IsAvtived = coupon.IsActived
                };

                if (couponDTO.CouponType == AppleEnum.CouponType.ORDER.ToString()) couponOrder.Add(couponDTO);
                if (couponDTO.CouponType == AppleEnum.CouponType.SHIP.ToString()) couponShip.Add(couponDTO);
            }

            if (user is not null && user.Role == 0)
            {
                var existedCoupon = userCouponRepository.FindAll(x => x.UserId == user.Id, false).ToList();

                couponOrder.RemoveAll(c => existedCoupon.Select(x => x.CouponId).Contains(c.Id));
                couponShip.RemoveAll(c => existedCoupon.Select(x => x.CouponId).Contains(c.Id));
            }

            var groupedCoupons = new CouponGroupedDTO
            {
                CouponOrder = couponOrder,
                CouponShip = couponShip
            };

            return Result<CouponGroupedDTO>.Ok(groupedCoupons);
        }
    }
}