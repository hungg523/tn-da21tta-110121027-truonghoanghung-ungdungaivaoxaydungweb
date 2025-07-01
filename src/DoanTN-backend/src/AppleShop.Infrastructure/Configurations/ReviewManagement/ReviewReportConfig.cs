using AppleShop.Domain.Constant.ReviewManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ReviewManagement
{
    public class ReviewReportConfig : IEntityTypeConfiguration<ReviewReport>
    {
        public void Configure(EntityTypeBuilder<ReviewReport> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ReviewReportConstant.FIELD_ID);
            builder.Property(x => x.ReviewId).HasColumnName(ReviewConstant.FIELD_ID);
            builder.Property(x => x.Status).HasColumnName(ReviewReportConstant.FIELD_STATUS);
            builder.Property(x => x.TotalReports).HasColumnName(ReviewReportConstant.FIELD_TOTAL_REPORTS);
            builder.Property(x => x.CreatedAt).HasColumnName(ReviewReportConstant.FIELD_CREATED_AT);

            builder.HasMany(x => x.ReportUsers).WithOne().HasForeignKey(x => x.ReportId);
            builder.ToTable(ReviewReportConstant.TABLE_NAME);
        }
    }
}