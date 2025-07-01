using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AppleShop.Domain.Entities.UserManagement;
using AppleShop.Domain.Constant.UserManagement;

namespace AppleShop.Infrastructure.Configurations.UserManagement
{
    public class UserAddressConfig : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(UserAddressConstant.FIELD_ID);
            builder.Property(x => x.FirstName).HasColumnName(UserAddressConstant.FIELD_FIRST_NAME);
            builder.Property(x => x.LastName).HasColumnName(UserAddressConstant.FIELD_LAST_NAME);
            builder.Property(x => x.AddressLine).HasColumnName(UserAddressConstant.FIELD_ADDRESS_LINE);
            builder.Property(x => x.PhoneNumber).HasColumnName(UserAddressConstant.FIELD_PHONE_NUMBER);
            builder.Property(x => x.Province).HasColumnName(UserAddressConstant.FIELD_PROVINCE);
            builder.Property(x => x.District).HasColumnName(UserAddressConstant.FIELD_DISTRICT);
            builder.Property(x => x.Ward).HasColumnName(UserAddressConstant.FIELD_WARD);
            builder.Property(x => x.Default).HasColumnName(UserAddressConstant.FIELD_DEFAULT);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.ToTable(UserAddressConstant.TABLE_NAME);
        }
    }
}