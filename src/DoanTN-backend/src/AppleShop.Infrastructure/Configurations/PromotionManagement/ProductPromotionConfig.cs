using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Constant.PromotionManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.PromotionManagement
{
    public class ProductPromotionConfig : IEntityTypeConfiguration<ProductPromotion>
    {
        public void Configure(EntityTypeBuilder<ProductPromotion> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ProductPromotionConstant.FIELD_ID);
            builder.Property(x => x.ProductId).HasColumnName(ProductConstant.FIELD_ID);
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.PromotionId).HasColumnName(PromotionConstant.FIELD_ID);

            builder.ToTable(ProductPromotionConstant.TABLE_NAME);
        }
    }
}