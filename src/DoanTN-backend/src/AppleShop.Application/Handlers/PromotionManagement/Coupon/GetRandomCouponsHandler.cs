using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Application.Requests.PromotionManagement.Coupon;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Shared;
using MediatR;
using AppleEnum = AppleShop.Share.Enumerations;

namespace AppleShop.Application.Handlers.PromotionManagement.Coupon
{
    public class GetRandomCouponsHandler : IRequestHandler<GetRandomCouponsRequest, Result<List<CouponDTO>>>
    {
        private readonly ICouponRepository couponRepository;
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IUserRepository userRepository;
        private readonly IUserCouponRepository userCouponRepository;

        public GetRandomCouponsHandler(ICouponRepository couponRepository, ICouponTypeRepository couponTypeRepository, IUserRepository userRepository, IUserCouponRepository userCouponRepository)
        {
            this.couponRepository = couponRepository;
            this.couponTypeRepository = couponTypeRepository;
            this.userRepository = userRepository;
            this.userCouponRepository = userCouponRepository;
        }

        public async Task<Result<List<CouponDTO>>> Handle(GetRandomCouponsRequest request, CancellationToken cancellationToken)
        {
                var user = await userRepository.FindByIdAsync(request.UserId, false);
                var validCoupons = couponRepository.FindAll(x => x.IsActived == true && x.EndDate > DateTime.Now && x.TimesUsed < x.MaxUsage).ToList();
                List<int?> existedIds = new();
                if (user is not null && user.Role == 0)
                {
                    var existedCoupon = userCouponRepository.FindAll(x => x.UserId == user.Id, false).ToList();
                    existedIds = existedCoupon.Select(x => x.CouponId).ToList();
                }
                var couponTypes = couponTypeRepository.FindAll(x => validCoupons.Select(x => x.CtId).Contains(x.Id)).ToDictionary(ct => ct.Id, ct => ct.Name);
                var randomCouponDtos = validCoupons.Where(x => !existedIds.Contains(x.Id)).Select(x => new CouponDTO
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    DiscountAmount = x.DiscountAmount,
                    DiscountPercentage = x.DiscountPercentage,
                    CouponType = ((AppleEnum.CouponType)couponTypes[x.CtId]).ToString(),
                    MinOrderValue = x.MinOrderValue,
                    MaxDiscountAmount = x.MaxDiscountAmount,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TimesUsed = x.TimesUsed,
                    MaxUsage = x.MaxUsage
                }).ToList();

                var random = new Random();
                var randomCoupons = randomCouponDtos.OrderBy(x => random.Next()).Take(5).ToList();

                randomCoupons.Add(new CouponDTO
                {
                    Id = null,
                    Code = "Không có mã giảm giá",
                    Description = "Không có mã giảm giá nào được áp dụng",
                    DiscountAmount = 0,
                    DiscountPercentage = 0,
                    MinOrderValue = 0,
                    MaxDiscountAmount = 0,
                    StartDate = null,
                    EndDate = null,
                    TimesUsed = 0,
                    MaxUsage = 0
                });

                return Result<List<CouponDTO>>.Ok(randomCoupons);
        }
    }
}