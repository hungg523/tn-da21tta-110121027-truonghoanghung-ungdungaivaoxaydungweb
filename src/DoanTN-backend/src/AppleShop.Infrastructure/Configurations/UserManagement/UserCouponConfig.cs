using AppleShop.Domain.Constant.PromotionManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.UserManagement
{
    public class UserCouponConfig : IEntityTypeConfiguration<UserCoupon>
    {
        public void Configure(EntityTypeBuilder<UserCoupon> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(UserCouponConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.CouponId).HasColumnName(CouponConstant.FIELD_ID);
            builder.Property(x => x.IsUsed).HasColumnName(UserCouponConstant.FIELD_IS_USED);
            builder.Property(x => x.TimesUsed).HasColumnName(UserCouponConstant.FIELD_TIMES_USED);
            builder.Property(x => x.ClaimedAt).HasColumnName(UserCouponConstant.FIELD_CLAIMED_AT);

            builder.ToTable(UserCouponConstant.TABLE_NAME);

        }
    }
}