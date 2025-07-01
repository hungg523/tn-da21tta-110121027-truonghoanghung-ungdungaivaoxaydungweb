//using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;

//namespace AppleShop.Share.BackgroundJob
//{
//    public class ExpiredVoucherCleanupJob
//    {
//        private readonly ICouponRepository couponRepository;

//        public ExpiredVoucherCleanupJob(ICouponRepository couponRepository)
//        {
//            this.couponRepository = couponRepository;
//        }

//        public async Task Run()
//        {
//            var expiredCoupons = couponRepository.FindAll(x => x.EndDate < DateTime.Now || x.TimesUsed == x.MaxUsage).ToList();
//            if (expiredCoupons.Any())
//            {
//                expiredCoupons.ForEach(x => x.IsActived = false);
//                couponRepository.UpdateRange(expiredCoupons);
//                await couponRepository.SaveChangesAsync();
//            }
//        }
//    }
//}