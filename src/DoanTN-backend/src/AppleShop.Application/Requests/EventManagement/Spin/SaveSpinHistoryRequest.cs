using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.EventManagement.Spin
{
    public class SaveSpinHistoryRequest : ICommand
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? CouponId { get; set; }
    }
}