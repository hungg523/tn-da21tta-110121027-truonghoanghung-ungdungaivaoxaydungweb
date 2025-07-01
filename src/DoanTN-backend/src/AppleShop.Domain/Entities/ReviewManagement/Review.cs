using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.ReviewManagement
{
    public class Review : BaseEntity
    {
        public int? UserId { get; set; }
        public int? VariantId { get; set; }
        public int? OiId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsFlagged { get; set; }
        public bool? IsDeleted { get; set; }

        [JsonIgnore]
        public ICollection<ReviewMedia>? Media { get; set; }
    }
}