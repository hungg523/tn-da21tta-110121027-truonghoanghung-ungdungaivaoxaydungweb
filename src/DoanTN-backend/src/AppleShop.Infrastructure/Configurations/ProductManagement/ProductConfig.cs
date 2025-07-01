using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ProductConstant.FIELD_ID);
            builder.Property(x => x.Name).HasColumnName(ProductConstant.FIELD_NAME);
            builder.Property(x => x.Description).HasColumnName(ProductConstant.FIELD_DESCRIPTION);
            builder.Property(x => x.CreatedDate).HasColumnName(ProductConstant.FIELD_CREATED_DATE);
            builder.Property(x => x.CategoryId).HasColumnName(ProductConstant.FIELD_CATEGORY_ID);
            builder.Property(x => x.IsActived).HasColumnName(ProductConstant.FIELD_IS_ACTIVE);

            builder.ToTable(ProductConstant.TABLE_NAME);
        }
    }
}