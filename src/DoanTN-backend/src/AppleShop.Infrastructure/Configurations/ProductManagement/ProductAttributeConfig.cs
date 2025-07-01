using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Domain.Constant.ProductManagement;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class ProductAttributeConfig : IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.Ignore(x => x.Id);
            builder.HasKey(x => new
            {
                x.VariantId,
                x.AvId
            });
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.AvId).HasColumnName(AttributeValueConstant.FIELD_ID);

            builder.ToTable(ProductAttributeConstant.TABLE_NAME);
        }
    }
}