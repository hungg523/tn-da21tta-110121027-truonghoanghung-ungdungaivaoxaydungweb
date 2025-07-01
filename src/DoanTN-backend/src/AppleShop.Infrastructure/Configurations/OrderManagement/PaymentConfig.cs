using AppleShop.Domain.Constant.OrderManagement;
using AppleShop.Domain.Entities.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.OrderManagement
{
    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(PaymentConstant.FIELD_ID);
            builder.Property(x => x.OrderId).HasColumnName(OrderConstant.FIELD_ID);
            builder.Property(x => x.PaymentMethod).HasColumnName(PaymentConstant.FIELD_PAYMENT_METHOD);
            builder.Property(x => x.TransactionCode).HasColumnName(PaymentConstant.FIELD_TRANSACTION_CODE);
            builder.Property(x => x.Amount).HasColumnName(PaymentConstant.FIELD_AMOUNT);
            builder.Property(x => x.Status).HasColumnName(PaymentConstant.FIELD_STATUS);
            builder.Property(x => x.CreatedAt).HasColumnName(PaymentConstant.FIELD_CREATED_AT);
            builder.Property(x => x.UpdatedAt).HasColumnName(PaymentConstant.FIELD_UPDATED_AT);

            builder.ToTable(PaymentConstant.TABLE_NAME);
        }
    }
}