using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Constant.WishListManagement;
using AppleShop.Domain.Entities.WishListManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.WishListManagement
{
    public class WishListConfig : IEntityTypeConfiguration<WishList>
    {
        public void Configure(EntityTypeBuilder<WishList> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(WishListConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.AddedDate).HasColumnName(WishListConstant.FIELD_ADDED_DATE);
            builder.Property(x => x.IsActived).HasColumnName(WishListConstant.FIELD_IS_ACTIVED);
            builder.ToTable(WishListConstant.TABLE_NAME);
        }
    }
}