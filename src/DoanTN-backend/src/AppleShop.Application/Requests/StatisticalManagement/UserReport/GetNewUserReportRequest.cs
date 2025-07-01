using AppleShop.Application.Requests.DTOs.StatisticalManagement.UserReport;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.StatisticalManagement.UserReport
{
    public class GetNewUserReportRequest : IQuery<List<NewUserReportDTO>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}