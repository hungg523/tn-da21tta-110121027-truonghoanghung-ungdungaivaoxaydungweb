using AppleShop.Domain.Constant.AIManagement;
using AppleShop.Domain.Entities.AIManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppleShop.Infrastructure.Configurations.AIManagement
{
    public class AIPromptConfig : IEntityTypeConfiguration<AIPrompt>
    {
        public void Configure(EntityTypeBuilder<AIPrompt> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(AIPromptConstant.FIELD_ID);
            builder.Property(x => x.Name).HasColumnName(AIPromptConstant.FIELD_NAME);
            builder.Property(x => x.Content).HasColumnName(AIPromptConstant.FIELD_CONTENT);
            builder.Property(x => x.CreatedAt).HasColumnName(AIPromptConstant.FIELD_CREATED_AT);

            builder.ToTable(AIPromptConstant.TABLE_NAME);
        }
    }
}