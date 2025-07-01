using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.OrderManagement.Cart
{
    public class DeleteProductFromCartRequest : ICommand
    {
        [JsonIgnore]
        public int? UserId { get; set; }

        [JsonIgnore]
        public int? VariantId { get; set; }
    }
}