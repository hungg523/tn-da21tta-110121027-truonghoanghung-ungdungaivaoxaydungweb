using AppleShop.Application.Requests.DTOs.StatisticalManagement.OrderReport;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.StatisticalManagement.OrderReport
{
    public class GetOrderReportRequest : IQuery<List<OrderReportDTO>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TimeUnit { get; set; } // "day", "month", "year"
    }
} 