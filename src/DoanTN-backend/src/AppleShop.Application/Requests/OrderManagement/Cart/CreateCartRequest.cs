using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.OrderManagement.Cart
{
    public class CreateCartRequest : ICommand
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? VariantId { get; set; }
        public int? Quantity { get; set; }
    }
}