using AppleShop.Application.Requests.DTOs.ReviewManagement.ReviewReport;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ReviewManagement.ReviewReport
{
    public class GetAllReportRequest : IQuery<List<ReviewReportDTO>>
    {
    }
}