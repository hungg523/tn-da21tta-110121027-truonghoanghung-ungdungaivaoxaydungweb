using AppleShop.Domain.Constant.EventManagement;
using AppleShop.Domain.Entities.EventManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.EventManagemet
{
    public class BannerConfig : IEntityTypeConfiguration<Banner>
    {
        public void Configure(EntityTypeBuilder<Banner> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(BannerConstant.FIELD_ID);
            builder.Property(x => x.Title).HasColumnName(BannerConstant.FIELD_TITLE);
            builder.Property(x => x.Description).HasColumnName(BannerConstant.FIELD_DESCRIPTION);
            builder.Property(x => x.Url).HasColumnName(BannerConstant.FIELD_URL);
            builder.Property(x => x.Link).HasColumnName(BannerConstant.FIELD_LINK);
            builder.Property(x => x.StartDate).HasColumnName(BannerConstant.FIELD_START_DATE);
            builder.Property(x => x.EndDate).HasColumnName(BannerConstant.FIELD_END_DATE);
            builder.Property(x => x.Status).HasColumnName(BannerConstant.FIELD_STATUS);
            builder.Property(x => x.Position).HasColumnName(BannerConstant.FIELD_POSITION);

            builder.ToTable(BannerConstant.TABLE_NAME);
        }
    }
}