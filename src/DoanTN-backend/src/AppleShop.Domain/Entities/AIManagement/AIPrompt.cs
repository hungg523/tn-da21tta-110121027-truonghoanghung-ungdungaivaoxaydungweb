using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.AIManagement
{
    public class AIPrompt : BaseEntity
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}