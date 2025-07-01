using AppleShop.Domain.Constant.PromotionManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.PromotionManagement
{
    public class PromotionConfig : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(PromotionConstant.FIELD_ID);
            builder.Property(x => x.Name).HasColumnName(PromotionConstant.FIELD_NAME);
            builder.Property(x => x.Description).HasColumnName(PromotionConstant.FIELD_DESCRIPTION);
            builder.Property(x => x.DiscountPercentage).HasColumnName(PromotionConstant.FIELD_DISCOUNT_PERCENTAGE);
            builder.Property(x => x.DiscountAmout).HasColumnName(PromotionConstant.FIELD_DISCOUNT_AMOUNT);
            builder.Property(x => x.StartDate).HasColumnName(PromotionConstant.FIELD_START_DATE);
            builder.Property(x => x.EndDate).HasColumnName(PromotionConstant.FIELD_END_DATE);
            builder.Property(x => x.IsActived).HasColumnName(PromotionConstant.FIELD_IS_ACTIVED);
            builder.Property(x => x.IsFlashSale).HasColumnName(PromotionConstant.FIELD_IS_FLASH_SALE);

            builder.HasMany(x => x.ProductPromotions).WithOne().HasForeignKey(x => x.PromotionId);
            builder.ToTable(PromotionConstant.TABLE_NAME);
        }
    }
}