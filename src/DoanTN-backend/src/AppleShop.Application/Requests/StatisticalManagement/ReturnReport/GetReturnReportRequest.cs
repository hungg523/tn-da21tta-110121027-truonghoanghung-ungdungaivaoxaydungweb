using AppleShop.Application.Requests.DTOs.StatisticalManagement.ReturnReport;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.StatisticalManagement.ReturnReport
{
    public class GetReturnReportRequest : IQuery<List<ReturnReportDTO>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TimeUnit { get; set; } // "day", "month", "year"
        public int? TopReasonsCount { get; set; } = 5; // Số lượng lý do phổ biến muốn hiển thị
    }
}