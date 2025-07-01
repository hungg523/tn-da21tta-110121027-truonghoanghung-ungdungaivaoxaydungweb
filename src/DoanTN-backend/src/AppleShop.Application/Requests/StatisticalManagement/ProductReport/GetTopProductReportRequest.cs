using AppleShop.Application.Requests.DTOs.StatisticalManagement.ProductReport;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.StatisticalManagement.ProductReport
{
    public class GetTopProductReportRequest : IQuery<List<TopProductReportDTO>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Limit { get; set; } = 10;
        public string? Type { get; set; } // best-selling, most-viewed, most-carted, most-wished, highest-rated
    }
} 