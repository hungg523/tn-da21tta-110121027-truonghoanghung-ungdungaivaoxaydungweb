using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.ReviewManagement
{
    public class ReviewReport : BaseEntity
    {
        public int? ReviewId { get; set; }
        public int? Status { get; set; }
        public int? TotalReports { get; set; }
        public DateTime? CreatedAt { get; set; }

        [JsonIgnore]
        public ICollection<ReviewReportUser>? ReportUsers { get; set; }
    }
}