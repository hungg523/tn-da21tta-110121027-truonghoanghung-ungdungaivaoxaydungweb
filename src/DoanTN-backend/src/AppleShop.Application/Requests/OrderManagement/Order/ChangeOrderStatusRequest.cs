using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.OrderManagement.Order
{
    public class ChangeOrderStatusRequest : ICommand
    {
        [JsonIgnore]
        public int? OiId { get; set; }
        public int? ItemStatus { get; set; }
    }
}