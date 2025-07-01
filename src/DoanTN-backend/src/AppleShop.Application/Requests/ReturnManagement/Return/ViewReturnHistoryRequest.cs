using AppleShop.Application.Requests.DTOs.ReturnManagement.Return;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ReturnManagement.Return
{
    public class ViewReturnHistoryRequest : IQuery<ReturnHistoryResponseDTO>
    {
        public int? UserId { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}