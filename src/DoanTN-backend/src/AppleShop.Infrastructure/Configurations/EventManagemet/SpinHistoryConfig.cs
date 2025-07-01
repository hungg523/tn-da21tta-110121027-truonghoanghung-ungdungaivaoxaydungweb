using AppleShop.Domain.Constant.EventManagement;
using AppleShop.Domain.Constant.PromotionManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.EventManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.EventManagemet
{
    public class SpinHistoryConfig : IEntityTypeConfiguration<SpinHistory>
    {
        public void Configure(EntityTypeBuilder<SpinHistory> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(SpinHistoryConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.CouponId).HasColumnName(CouponConstant.FIELD_ID);
            builder.Property(x => x.SpinDate).HasColumnName(SpinHistoryConstant.FIELD_SPIN_DATE);
            builder.Property(x => x.CreatedAt).HasColumnName(SpinHistoryConstant.FIELD_CREATED_AT);

            builder.ToTable(SpinHistoryConstant.TABLE_NAME);
        }
    }
}