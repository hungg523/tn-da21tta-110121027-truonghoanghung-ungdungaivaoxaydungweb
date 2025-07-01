using AppleShop.Domain.Constant.CategoryManagement;
using AppleShop.Domain.Entities.CategoryManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.CategoryManagement
{
    public class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(CategoryConstant.FIELD_ID);
            builder.Property(x => x.Name).HasColumnName(CategoryConstant.FIELD_NAME);
            builder.Property(x => x.CatPid).HasColumnName(CategoryConstant.FIELD_CAT_PID);
            builder.Property(x => x.Description).HasColumnName(CategoryConstant.FIELD_DESCRIPTION);
            builder.Property(x => x.CreatedDate).HasColumnName(CategoryConstant.FIELD_CREATED_DATE);
            builder.Property(x => x.IsActived).HasColumnName(CategoryConstant.FIELD_IS_ACTIVE);
            builder.ToTable(CategoryConstant.TABLE_NAME);
        }
    }
}