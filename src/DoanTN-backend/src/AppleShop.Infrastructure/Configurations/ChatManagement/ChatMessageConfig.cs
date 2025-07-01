using AppleShop.Domain.Constant.ChatManagement;
using AppleShop.Domain.Entities.ChatManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ChatManagement
{
    public class ChatMessageConfig : IEntityTypeConfiguration<ChatMessages>
    {
        public void Configure(EntityTypeBuilder<ChatMessages> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(MessageConstant.FIELD_ID);
            builder.Property(x => x.ConversationId).HasColumnName(ConversationConstant.FIELD_ID);
            builder.Property(x => x.SenderType).HasColumnName(MessageConstant.FIELD_SENDER_TYPE);
            builder.Property(x => x.SenderId).HasColumnName(MessageConstant.FIELD_SENDER_ID);
            builder.Property(x => x.Message).HasColumnName(MessageConstant.FIELD_MESSAGE);
            builder.Property(x => x.TimeStamp).HasColumnName(MessageConstant.FIELD_TIME_STAMP);
            builder.Property(x => x.IsFromBot).HasColumnName(MessageConstant.FIELD_IS_FROM_BOT);

            builder.ToTable(MessageConstant.TABLE_NAME);
        }
    }
}