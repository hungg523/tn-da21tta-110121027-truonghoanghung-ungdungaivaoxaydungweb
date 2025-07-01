using AppleShop.Domain.Constant.PromotionManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.PromotionManagement
{
    public class CouponConfig : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(CouponConstant.FIELD_ID);
            builder.Property(x => x.Code).HasColumnName(CouponConstant.FIELD_CODE);
            builder.Property(x => x.Description).HasColumnName(CouponConstant.FIELD_DESCRIPTION);
            builder.Property(x => x.DiscountPercentage).HasColumnName(CouponConstant.FIELD_DISCOUNT_PERCENTAGE);
            builder.Property(x => x.DiscountAmount).HasColumnName(CouponConstant.FIELD_DISCOUNT_AMOUNT);
            builder.Property(x => x.MaxDiscountAmount).HasColumnName(CouponConstant.FIELD_MAX_DISCOUNT_AMOUNT);
            builder.Property(x => x.MinOrderValue).HasColumnName(CouponConstant.FIELD_MIN_ORDER_VALUE);
            builder.Property(x => x.TimesUsed).HasColumnName(CouponConstant.FIELD_TIMES_USED);
            builder.Property(x => x.MaxUsage).HasColumnName(CouponConstant.FIELD_MAX_USED);
            builder.Property(x => x.MaxUsagePerUser).HasColumnName(CouponConstant.FIELD_MAX_USAGE_PER_USER);
            builder.Property(x => x.IsVip).HasColumnName(CouponConstant.FIELD_IS_VIP);
            builder.Property(x => x.UserSpecific).HasColumnName(CouponConstant.FIELD_USER_SPECIFIC);
            builder.Property(x => x.Terms).HasColumnName(CouponConstant.FIELD_TERMS);
            builder.Property(x => x.CtId).HasColumnName(CouponTypeConstant.FIELD_ID);
            builder.Property(x => x.StartDate).HasColumnName(CouponConstant.FIELD_START_DATE);
            builder.Property(x => x.EndDate).HasColumnName(CouponConstant.FIELD_END_DATE);
            builder.Property(x => x.CreatedAt).HasColumnName(CouponConstant.FIELD_CREATE_DATE);
            builder.Property(x => x.IsActived).HasColumnName(CouponConstant.FIELD_IS_ACTIVED);

            builder.ToTable(CouponConstant.TABLE_NAME);
        }
    }
}