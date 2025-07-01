using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class ProductImageConfig : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ProductImageConstant.FIELD_ID);
            builder.Property(x => x.Title).HasColumnName(ProductImageConstant.FIELD_TITLE);
            builder.Property(x => x.Url).HasColumnName(ProductImageConstant.FIELD_URL);
            builder.Property(x => x.Position).HasColumnName(ProductImageConstant.FIELD_POSITION);
            builder.Property(x => x.ProductId).HasColumnName(ProductImageConstant.FIELD_PRDUCT_ID);
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.ToTable(ProductImageConstant.TABLE_NAME);
        }
    }
}