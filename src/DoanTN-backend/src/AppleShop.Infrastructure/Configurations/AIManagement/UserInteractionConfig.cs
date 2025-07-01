using AppleShop.Domain.Constant.AIManagement;
using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.AIManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.AIManagement
{
    public class UserInteractionConfig : IEntityTypeConfiguration<UserInteraction>
    {
        public void Configure(EntityTypeBuilder<UserInteraction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(UserInteractionConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.Type).HasColumnName(UserInteractionConstant.FIELD_TYPE);
            builder.Property(x => x.Value).HasColumnName(UserInteractionConstant.FIELD_VALUE);
            builder.Property(x => x.CreatedAt).HasColumnName(UserInteractionConstant.FIELD_CREATED_AT);

            builder.ToTable(UserInteractionConstant.TABLE_NAME);
        }
    }
}