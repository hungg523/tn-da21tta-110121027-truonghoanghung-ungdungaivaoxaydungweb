using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.UserManagement
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.Username).HasColumnName(UserConstant.FIELD_USERNAME);
            builder.Property(x => x.Email).HasColumnName(UserConstant.FIELD_EMAIL);
            builder.Property(x => x.Password).HasColumnName(UserConstant.FIELD_PASSWORD);
            builder.Property(x => x.Gender).HasColumnName(UserConstant.FIELD_GENDER);
            builder.Property(x => x.DateOfBirth).HasColumnName(UserConstant.FIELD_DATE_OF_BIRTH);
            builder.Property(x => x.ImageUrl).HasColumnName(UserConstant.FIELD_IMAGE_URL);
            builder.Property(x => x.OTP).HasColumnName(UserConstant.FIELD_OTP);
            builder.Property(x => x.OTPAttempt).HasColumnName(UserConstant.FIELD_OTP_ATTEMPT);
            builder.Property(x => x.OTPExpiration).HasColumnName(UserConstant.FIELD_OTP_EXPIRATION);
            builder.Property(x => x.CreatedAt).HasColumnName(UserConstant.FIELD_CREATED_DATE);
            builder.Property(x => x.LastLogin).HasColumnName(UserConstant.FIELD_LAST_LOGIN);
            builder.Property(x => x.IsActived).HasColumnName(UserConstant.FIELD_IS_ACTIVED);
            builder.ToTable(UserConstant.TABLE_NAME);
        }
    }
}