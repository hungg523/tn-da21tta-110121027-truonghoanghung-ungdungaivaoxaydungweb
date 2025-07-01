using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class ProductDetailConfig : IEntityTypeConfiguration<ProductDetail>
    {
        public void Configure(EntityTypeBuilder<ProductDetail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ProductDetailConstant.FIELD_ID);
            builder.Property(x => x.ProductId).HasColumnName(ProductConstant.FIELD_ID);
            builder.Property(x => x.DetailKey).HasColumnName(ProductDetailConstant.FIELD_DETAIL_KEY);
            builder.Property(x => x.DetailValue).HasColumnName(ProductDetailConstant.FIELD_DETAIL_VALUE);

            builder.ToTable(ProductDetailConstant.TABLE_NAME);
        }
    }
}