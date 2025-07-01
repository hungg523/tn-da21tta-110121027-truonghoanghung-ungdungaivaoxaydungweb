using AppleShop.Domain.Constant.ReviewManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ReviewManagement
{
    public class ReviewMediaConfig : IEntityTypeConfiguration<ReviewMedia>
    {
        public void Configure(EntityTypeBuilder<ReviewMedia> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ReviewMediaConstant.FIELD_ID);
            builder.Property(x => x.ReviewId).HasColumnName(ReviewConstant.FIELD_ID);
            builder.Property(x => x.Url).HasColumnName(ReviewMediaConstant.FIELD_URL);

            builder.ToTable(ReviewMediaConstant.TABLE_NAME);
        }
    }
}