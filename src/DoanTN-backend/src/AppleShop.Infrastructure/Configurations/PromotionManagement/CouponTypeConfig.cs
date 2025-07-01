using AppleShop.Domain.Constant.PromotionManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.PromotionManagement
{
    public class CouponTypeConfig : IEntityTypeConfiguration<CouponType>
    {
        public void Configure(EntityTypeBuilder<CouponType> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(CouponTypeConstant.FIELD_ID);
            builder.Property(x => x.Name).HasColumnName(CouponTypeConstant.FIELD_NAME);
            builder.Property(x => x.Description).HasColumnName(CouponTypeConstant.FIELD_DESCRIPTION);

            builder.ToTable(CouponTypeConstant.TABLE_NAME);
        }
    }
}