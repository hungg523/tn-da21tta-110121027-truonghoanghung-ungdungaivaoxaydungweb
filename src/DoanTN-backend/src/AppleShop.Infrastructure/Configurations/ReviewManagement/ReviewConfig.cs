using AppleShop.Domain.Constant.OrderManagement;
using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Constant.ReviewManagement;
using AppleShop.Domain.Constant.UserManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ReviewManagement
{
    public class ReviewConfig : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(ReviewConstant.FIELD_ID);
            builder.Property(x => x.UserId).HasColumnName(UserConstant.FIELD_ID);
            builder.Property(x => x.VariantId).HasColumnName(ProductVariantConstant.FIELD_ID);
            builder.Property(x => x.OiId).HasColumnName(OrderItemConstant.FIELD_ID);
            builder.Property(x => x.Rating).HasColumnName(ReviewConstant.FIELD_RATING);
            builder.Property(x => x.Comment).HasColumnName(ReviewConstant.FIELD_COMMENT);
            builder.Property(x => x.CreatedAt).HasColumnName(ReviewConstant.FIELD_CREATED_AT);
            builder.Property(x => x.IsFlagged).HasColumnName(ReviewConstant.FIELD_IS_FLAGGED);
            builder.Property(x => x.IsDeleted).HasColumnName(ReviewConstant.FIELD_IS_DELETED);

            builder.HasMany(x => x.Media).WithOne().HasForeignKey(x => x.ReviewId);
            builder.ToTable(ReviewConstant.TABLE_NAME);
        }
    }
}