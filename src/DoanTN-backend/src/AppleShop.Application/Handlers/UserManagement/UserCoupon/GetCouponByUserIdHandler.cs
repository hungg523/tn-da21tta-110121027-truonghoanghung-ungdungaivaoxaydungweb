using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Application.Requests.UserManagement.UserCoupon;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using AppleEnum = AppleShop.Share.Enumerations;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.UserCoupon
{
    public class GetCouponByUserIdHandler : IRequestHandler<GetCouponByUserIdRequest, Result<CouponGroupedDTO>>
    {
        private readonly ICouponRepository couponRepository;
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IUserRepository userRepository;
        private readonly IUserCouponRepository userCouponRepository;

        public GetCouponByUserIdHandler(ICouponRepository couponRepository, IUserCouponRepository userCouponRepository, IUserRepository userRepository, ICouponTypeRepository couponTypeRepository)
        {
            this.couponRepository = couponRepository;
            this.userCouponRepository = userCouponRepository;
            this.userRepository = userRepository;
            this.couponTypeRepository = couponTypeRepository;
        }

        public async Task<Result<CouponGroupedDTO>> Handle(GetCouponByUserIdRequest request, CancellationToken cancellationToken)
        {
            var user = await userRepository.FindSingleAsync(x => x.Id == request.UserId);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            var userCoupons = userCouponRepository.FindAll(x => x.UserId == request.UserId).ToList();
            var couponIds = userCoupons.Select(x => x.CouponId).ToList();

            using var transaction = await userCouponRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var expiredCoupons = couponRepository.FindAll(x => x.EndDate < DateTime.Now || x.TimesUsed == x.MaxUsage || x.UserSpecific == false || userCoupons.Select(x => x.TimesUsed).Contains(x.MaxUsagePerUser)).ToList();
                var expireUserCoupons = userCouponRepository.FindAll(x => expiredCoupons.Select(x => x.Id).ToList().Contains(x.CouponId)).ToList();
                var invalidUserCoupons = userCoupons.Where(x => x.IsUsed == true).ToList();

                if (expireUserCoupons.Any() || invalidUserCoupons.Any())
                {
                    userCouponRepository.RemoveMultiple(expireUserCoupons);
                    userCouponRepository.RemoveMultiple(invalidUserCoupons);
                    await userCouponRepository.SaveChangesAsync(cancellationToken);
                }

                var coupons = couponRepository.FindAll(x => couponIds.Contains(x.Id) && x.EndDate >= DateTime.Now).ToList();
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
                        TimesUsed = coupon.TimesUsed,
                        MaxUsage = coupon.MaxUsage,
                        MaxUsagePerUser = coupon.MaxUsagePerUser,
                        CouponType = ((AppleEnum.CouponType)couponTypes[coupon.CtId]).ToString(),
                        AvailableDate = $"{(coupon.EndDate - coupon.StartDate).Value.Days} ngày",
                        DiscountDisplay = coupon.DiscountPercentage > 0
                            ? $"Giảm {coupon.DiscountPercentage}% cho đơn từ {coupon.MinOrderValue} VND (Tối đa {coupon.MaxDiscountAmount} VND)"
                            : $"Giảm {coupon.DiscountAmount} VND cho đơn từ {coupon.MinOrderValue} VND",
                        Term = coupon.Terms,
                        IsVip = coupon.IsVip
                    };

                    if (couponDTO.CouponType == AppleEnum.CouponType.ORDER.ToString()) couponOrder.Add(couponDTO);
                    if (couponDTO.CouponType == AppleEnum.CouponType.SHIP.ToString()) couponShip.Add(couponDTO);
                }

                var groupedCoupons = new CouponGroupedDTO
                {
                    CouponOrder = couponOrder,
                    CouponShip = couponShip
                };

                transaction.Commit();
                return Result<CouponGroupedDTO>.Ok(groupedCoupons);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}