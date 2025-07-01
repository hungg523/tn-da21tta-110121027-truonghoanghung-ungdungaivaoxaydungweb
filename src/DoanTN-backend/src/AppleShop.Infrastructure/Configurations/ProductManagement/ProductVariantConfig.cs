using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class ProductVariantConfig : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.ProductId).HasColumnName(ProductConstant.FIELD_ID);
            builder.Property(x => x.Price).HasColumnName(ProductConstant.FIELD_PRICE);
            builder.Property(x => x.Stock).HasColumnName(ProductVariantConstant.FIELD_STOCK);
            builder.Property(x => x.ReservedStock).HasColumnName(ProductVariantConstant.FIELD_RESERVED_STOCK);
            builder.Property(x => x.SoldQuantity).HasColumnName(ProductVariantConstant.FIELD_SOLD_QUANTITY);
            builder.Property(x => x.IsActived).HasColumnName(ProductVariantConstant.FIELD_IS_ACTIVED);

            builder.HasMany(x => x.ProductAttributes).WithOne().HasForeignKey(x => x.VariantId);
            builder.ToTable(ProductVariantConstant.TABLE_NAME);
        }
    }
}