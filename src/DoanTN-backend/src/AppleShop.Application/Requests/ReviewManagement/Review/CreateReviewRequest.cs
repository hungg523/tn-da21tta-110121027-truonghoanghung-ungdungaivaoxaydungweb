using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReviewManagement.Review
{
    public class CreateReviewRequest : ICommand
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? VariantId { get; set; }
        public int? OiId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }

        [JsonIgnore]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public bool? IsDeleted { get; set; } = false;

        [JsonIgnore]
        public bool? IsFlagged { get; set; } = false;
        public ICollection<string>? ImageDatas { get; set; }
    }
}