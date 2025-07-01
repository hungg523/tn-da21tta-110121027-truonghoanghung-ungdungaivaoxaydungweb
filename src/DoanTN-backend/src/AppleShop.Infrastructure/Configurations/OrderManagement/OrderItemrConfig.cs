using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Domain.Constant.OrderManagement;

namespace AppleShop.Infrastructure.Configurations.OrderManagement
{
    public class OrderItemrConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.Property(x => x.Id).HasColumnName(OrderItemConstant.FIELD_ID);
            builder.Property(x => x.VariantId).HasColumnName(OrderItemConstant.FIELD_VARIANT_ID);
            builder.Property(x => x.OrderId).HasColumnName(OrderConstant.FIELD_ID);
            builder.Property(x => x.Quantity).HasColumnName(OrderItemConstant.FIELD_QUANTITY);
            builder.Property(x => x.OriginalPrice).HasColumnName(OrderItemConstant.FIELD_ORIGINAL_PRICE);
            builder.Property(x => x.FinalPrice).HasColumnName(OrderItemConstant.FIELD_FINAL_PRICE);
            builder.Property(x => x.TotalPrice).HasColumnName(OrderItemConstant.FIELD_TOTAL_PRICE);
            builder.Property(x => x.ItemStatus).HasColumnName(OrderItemConstant.FIELD_ITEM_STATUS);
            builder.Property(x => x.IsReview).HasColumnName(OrderItemConstant.FIELD_IS_REVIEW);

            builder.ToTable(OrderItemConstant.TABLE_NAME);
        }
    }
}