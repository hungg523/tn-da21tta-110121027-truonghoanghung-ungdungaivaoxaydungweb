using AppleShop.Domain.Constant.OrderManagement;
using AppleShop.Domain.Constant.ReturnManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.ReturnManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ReturnManagement
{
    public class ReturnConfig : IEntityTypeConfiguration<Return>
    {
        public void Configure(EntityTypeBuilder<Return> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ReturnConstant.FIELD_ID);
            builder.Property(x => x.OiId).HasColumnName(OrderItemConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.Reason).HasColumnName(ReturnConstant.FIELD_REASON);
            builder.Property(x => x.Quantity).HasColumnName(ReturnConstant.FIELD_QUANTITY);
            builder.Property(x => x.RefundAmount).HasColumnName(ReturnConstant.FIELD_REFUND_AMOUNT);
            builder.Property(x => x.Status).HasColumnName(ReturnConstant.FIELD_STATUS);
            builder.Property(x => x.CreatedAt).HasColumnName(ReturnConstant.FIELD_CREATED_AT);
            builder.Property(x => x.ProcessedAt).HasColumnName(ReturnConstant.FIELD_PROCESSED_AT);
            builder.Property(x => x.AccountName).HasColumnName(ReturnConstant.FIELD_ACCOUNT_NAME);
            builder.Property(x => x.AccountNumber).HasColumnName(ReturnConstant.FIELD_ACCOUNT_NUMBER);
            builder.Property(x => x.BankName).HasColumnName(ReturnConstant.FIELD_BANK_NAME);
            builder.Property(x => x.PhoneNumber).HasColumnName(ReturnConstant.FIELD_PHONE_NUMBER);
            builder.Property(x => x.ReturnType).HasColumnName(ReturnConstant.FIELD_RETURN_TYPE);
            builder.Property(x => x.Url).HasColumnName(ReturnConstant.FIELD_URL);

            builder.ToTable(ReturnConstant.TABLE_NAME);
        }
    }
}