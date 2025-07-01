using AppleShop.Application.Requests.DTOs.ReturnManagement.Return;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.ReturnManagement.Return
{
    public class GetDetailItemReturnRequest : IQuery<ReturnDetailDTO>
    {
        public int? ReturnId { get; set; }
    }
}