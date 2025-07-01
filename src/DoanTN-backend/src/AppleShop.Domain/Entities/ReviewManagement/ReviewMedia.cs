using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ReviewManagement
{
    public class ReviewMedia : BaseEntity
    {
        public int? ReviewId { get; set; }
        public string? Url { get; set; }
    }
}