using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;

namespace AppleShop.Application.Requests.DTOs.ReviewManagement.ReviewReport
{
    public class ReviewReportDTO
    {
        public int Id { get; set; }
        public ReviewDTO? Review { get; set; }
        public string? Status { get; set; }
        public int? TotalReports { get; set; }
        public List<ReportUserDTO>? ReportUsers { get; set; } = new List<ReportUserDTO>();
    }
}