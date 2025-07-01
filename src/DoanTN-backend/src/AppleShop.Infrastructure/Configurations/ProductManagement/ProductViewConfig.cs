using AppleShop.Domain.Constant.CategoryManagement;
using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class ProductViewConfig : IEntityTypeConfiguration<ProductView>
    {
        public void Configure(EntityTypeBuilder<ProductView> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ProductViewConstant.FIELD_ID);
            builder.Property(x => x.ProductId).HasColumnName(ProductConstant.FIELD_ID);
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.CategoryId).HasColumnName(CategoryConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.View).HasColumnName(ProductViewConstant.FIELD_VIEW);
            builder.Property(x => x.CreatedAt).HasColumnName(ProductViewConstant.FIELD_CREATED_AT);
            builder.Property(x => x.UpdatedAt).HasColumnName(ProductViewConstant.FIELD_UPDATED_AT);

            builder.ToTable(ProductViewConstant.TABLE_NAME);
        }
    }
}