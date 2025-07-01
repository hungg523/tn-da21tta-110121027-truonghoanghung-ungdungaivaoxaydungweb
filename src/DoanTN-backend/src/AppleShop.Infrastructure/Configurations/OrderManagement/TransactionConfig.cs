using AppleShop.Domain.Constant.OrderManagement;
using AppleShop.Domain.Entities.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.OrderManagement
{
    public class TransactionConfig : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(TransactionConstant.FIELD_ID);
            builder.Property(x => x.OrderId).HasColumnName(OrderConstant.FIELD_ID);
            builder.Property(x => x.PaymentGateway).HasColumnName(TransactionConstant.FIELD_PAYMENT_GATEWAY);
            builder.Property(x => x.Code).HasColumnName(TransactionConstant.FIELD_CODE);
            builder.Property(x => x.Amount).HasColumnName(TransactionConstant.FIELD_AMOUNT);
            builder.Property(x => x.Status).HasColumnName(TransactionConstant.FIELD_STATUS);
            builder.Property(x => x.CreatedAt).HasColumnName(TransactionConstant.FIELD_CREATED_AT);
            builder.Property(x => x.UpdatedAt).HasColumnName(TransactionConstant.FIELD_UPDATED_AT);

            builder.ToTable(TransactionConstant.TABLE_NAME);
        }
    }
}