using AppleShop.Application.Requests.DTOs.StatisticalManagement.SummaryReport;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.StatisticalManagement.SummaryReport
{
    public class GetSummaryReportRequest : IQuery<List<SummaryReportDTO>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TimeUnit { get; set; } // "day", "month", "year"
    }
} 