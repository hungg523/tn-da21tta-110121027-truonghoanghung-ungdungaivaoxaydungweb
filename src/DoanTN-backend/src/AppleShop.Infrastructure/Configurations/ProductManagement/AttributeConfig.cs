using AppleShop.Domain.Constant.ProductManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Infrastructure.Configurations.ProductManagement
{
    public class AttributeConfig : IEntityTypeConfiguration<Entities.ProductManagement.Attribute>
    {
        public void Configure(EntityTypeBuilder<Entities.ProductManagement.Attribute> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(AttributeConstant.FIELD_ID);
            builder.Property(x => x.Name).HasColumnName(AttributeConstant.FIELD_NAME);

            builder.ToTable(AttributeConstant.TABLE_NAME);
        }
    }
}