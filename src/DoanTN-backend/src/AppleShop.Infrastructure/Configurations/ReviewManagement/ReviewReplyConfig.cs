using AppleShop.Domain.Constant.ReviewManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ReviewManagement
{
    public class ReviewReplyConfig : IEntityTypeConfiguration<ReviewReply>
    {
        public void Configure(EntityTypeBuilder<ReviewReply> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ReviewReplyConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.ReviewId).HasColumnName(ReviewConstant.FIELD_ID);
            builder.Property(x => x.ReplyText).HasColumnName(ReviewReplyConstant.FIELD_REPLY_TEXT);
            builder.Property(x => x.CreatedAt).HasColumnName(ReviewReplyConstant.FIELD_CREATED_AT);

            builder.ToTable(ReviewReplyConstant.TABLE_NAME);
        }
    }
}