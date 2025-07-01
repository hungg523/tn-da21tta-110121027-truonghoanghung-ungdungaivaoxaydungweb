using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReviewManagement.ReviewReport
{
    public class HandleReportRequest : ICommand
    {
        [JsonIgnore]
        public int? ReportId { get; set; }
        public int? Status { get; set; }
    }
}