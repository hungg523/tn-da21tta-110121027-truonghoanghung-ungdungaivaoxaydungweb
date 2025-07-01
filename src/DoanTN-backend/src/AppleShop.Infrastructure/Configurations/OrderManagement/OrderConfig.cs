using AppleShop.Domain.Constant.OrderManagement;
using AppleShop.Domain.Entities.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.OrderManagement
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(OrderConstant.FIELD_ID);
            builder.Property(x => x.OrderCode).HasColumnName(OrderConstant.FIELD_ORDER_CODE);
            builder.Property(x => x.Status).HasColumnName(OrderConstant.FIELD_STATUS);
            builder.Property(x => x.Payment).HasColumnName(OrderConstant.FIELD_PAYMENT);
            builder.Property(x => x.TotalAmount).HasColumnName(OrderConstant.FIELD_TOTAL_AMOUNT);
            builder.Property(x => x.ShipFee).HasColumnName(OrderConstant.FIELD_SHIP_FEE);
            builder.Property(x => x.UserId).HasColumnName(OrderConstant.FIELD_USER_ID);
            builder.Property(x => x.UserAddressId).HasColumnName(OrderConstant.FIELD_USER_ADDRESS_ID);
            builder.Property(x => x.CouponId).HasColumnName(OrderConstant.FIELD_COUPON_ID);
            builder.Property(x => x.ShipCouponId).HasColumnName(OrderConstant.FIELD_SHIP_COUPON_ID);
            builder.Property(x => x.CreatedAt).HasColumnName(OrderConstant.FIELD_CREATED_DATE);
            builder.Property(x => x.UpdatedAt).HasColumnName(OrderConstant.FIELD_UPDATED_DATE);

            builder.HasMany(x => x.OrderItems).WithOne().HasForeignKey(x => x.OrderId);
            builder.ToTable(OrderConstant.TABLE_NAME);
        }
    }
}