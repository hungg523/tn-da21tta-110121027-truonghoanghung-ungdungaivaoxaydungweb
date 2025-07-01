using AppleShop.Domain.Constant.ReviewManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ReviewManagement
{
    public class ReviewReportUserConfig : IEntityTypeConfiguration<ReviewReportUser>
    {
        public void Configure(EntityTypeBuilder<ReviewReportUser> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ReviewReportUserConstant.FIELD_ID);
            builder.Property(x => x.ReportId).HasColumnName(ReviewReportConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.Reason).HasColumnName(ReviewReportUserConstant.FIELD_REASON);
            builder.Property(x => x.CreatedAt).HasColumnName(ReviewReportUserConstant.FIELD_CREATED_AT);

            builder.ToTable(ReviewReportUserConstant.TABLE_NAME);
        }
    }
}