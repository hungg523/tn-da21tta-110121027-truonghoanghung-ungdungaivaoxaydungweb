using AppleShop.Domain.Constant.ProductManagement;
using AppleShop.Domain.Entities.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class AttributeValueConfig : IEntityTypeConfiguration<AttributeValue>
    {
        public void Configure(EntityTypeBuilder<AttributeValue> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(AttributeValueConstant.FIELD_ID);
            builder.Property(x => x.AttributeId).HasColumnName(AttributeConstant.FIELD_ID);
            builder.Property(x => x.Value).HasColumnName(AttributeValueConstant.FIELD_VALUE);

            builder.ToTable(AttributeValueConstant.TABLE_NAME);
        }
    }
}