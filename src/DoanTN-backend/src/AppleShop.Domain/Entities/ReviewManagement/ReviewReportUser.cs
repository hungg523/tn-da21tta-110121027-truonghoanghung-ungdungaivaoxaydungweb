using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.ReviewManagement
{
    public class ReviewReportUser : BaseEntity
    {
        public int? ReportId { get; set; }
        public int? UserId { get; set; }
        public string? Reason { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}