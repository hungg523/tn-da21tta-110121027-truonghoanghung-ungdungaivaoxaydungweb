using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Domain.Constant.OrderManagement;
using AppleShop.Domain.Constant.ProductManagement;

namespace AppleShop.Infrastructure.Configurations.OrderManagement
{
    public class CartItemrConfig : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.Property(x => x.Id).HasColumnName(CartItemConstant.FIELD_ID);
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.CartId).HasColumnName(CartConstant.FIELD_ID);
            builder.Property(x => x.Quantity).HasColumnName(CartItemConstant.FIELD_QUANTITY);
            builder.Property(x => x.UnitPrice).HasColumnName(CartItemConstant.FIELD_UNIT_PRICE);
            builder.Property(x => x.TotalPrice).HasColumnName(CartItemConstant.FIELD_TOTAL_PRICE);
            builder.ToTable(CartItemConstant.TABLE_NAME);

            builder.HasOne(x => x.Cart).WithMany(x => x.CartItems).HasForeignKey(x => x.CartId);
        }
    }
}