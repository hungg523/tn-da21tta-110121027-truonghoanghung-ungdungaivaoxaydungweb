using AppleShop.Domain.Constant.CategoryManagement;
using AppleShop.Domain.Constant.ChatManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.ChatManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ChatManagement
{
    public class ConversationConfig : IEntityTypeConfiguration<Conversations>
    {
        public void Configure(EntityTypeBuilder<Conversations> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ConversationConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.Status).HasColumnName(ConversationConstant.FIELD_STATUS);
            builder.Property(x => x.IsBotHandled).HasColumnName(ConversationConstant.FIELD_IS_BOT_HANDLED);
            builder.Property(x => x.CreatedAt).HasColumnName(ConversationConstant.FIELD_CREATED_AT);

            builder.ToTable(ConversationConstant.TABLE_NAME);
        }
    }
}