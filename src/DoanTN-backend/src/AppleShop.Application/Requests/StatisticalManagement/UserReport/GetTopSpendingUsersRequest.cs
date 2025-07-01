using AppleShop.Application.Requests.DTOs.StatisticalManagement.UserReport;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.StatisticalManagement.UserReport
{
    public class GetTopSpendingUsersRequest : IQuery<List<TopSpendingUserDTO>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TopCount { get; set; } = 5;
    }
} 