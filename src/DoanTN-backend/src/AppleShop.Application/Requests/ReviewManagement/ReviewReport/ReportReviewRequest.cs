using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReviewManagement.ReviewReport
{
    public class ReportReviewRequest : ICommand
    {
        public int? ReviewId { get; set; }
        
        [JsonIgnore]
        public int? UserId { get; set; }
        public string? Reason { get; set; }

        [JsonIgnore]
        public int? Status { get; set; } = (int)ReportStatus.Pending;

        [JsonIgnore]
        public int? TotalReports { get; set; } = 1;

        [JsonIgnore]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}