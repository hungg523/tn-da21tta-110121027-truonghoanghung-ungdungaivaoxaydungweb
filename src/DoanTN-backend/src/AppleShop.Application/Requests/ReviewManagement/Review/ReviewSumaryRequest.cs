using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReviewManagement.Review
{
    public class ReviewSumaryRequest : IQuery<ReviewSummaryDTO>
    {
        [JsonIgnore]
        public int? VariantId { get; set; }
    }
}