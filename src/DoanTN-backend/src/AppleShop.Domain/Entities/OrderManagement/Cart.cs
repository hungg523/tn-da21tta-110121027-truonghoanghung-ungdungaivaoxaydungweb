using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.OrderManagement
{
    public class Cart : BaseEntity
    {
        public int? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public ICollection<CartItem>? CartItems { get; set; }
    }
}