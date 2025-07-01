using AppleShop.Domain.Constant.OrderManagement;
using AppleShop.Domain.Entities.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.OrderManagement
{
    public class CartConfig : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(CartConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(CartConstant.FIELD_USER_ID);
            builder.Property(x => x.CreatedAt).HasColumnName(CartConstant.FIELD_CREATED_DATE);
            builder.Property(x => x.UpdatedAt).HasColumnName(CartConstant.FIELD_UPDATED_DATE);

            builder.HasMany(x => x.CartItems).WithOne().HasForeignKey(x => x.CartId);
            builder.ToTable(CartConstant.TABLE_NAME);
        }
    }
}