using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.AIManagement
{
    public class UserInteraction : BaseEntity
    {
        public int? UserId { get; set; }
        public int? VariantId { get; set; }
        public string? Type { get; set; }
        public float? Value { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}