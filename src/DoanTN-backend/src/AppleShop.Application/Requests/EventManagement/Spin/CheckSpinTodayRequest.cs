using AppleShop.Application.Requests.DTOs.EventManagement.Spin;
using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.EventManagement.Spin
{
    public class CheckSpinTodayRequest : IQuery<SpinDTO>
    {
        [JsonIgnore]
        public int? UserId { get; set; }
    }
}