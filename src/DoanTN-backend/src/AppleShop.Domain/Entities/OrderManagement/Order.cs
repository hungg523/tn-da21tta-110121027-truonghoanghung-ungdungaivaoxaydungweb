using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.OrderManagement
{
    public class Order : BaseEntity
    {
        public string? OrderCode { get; set; }
        public int? Status { get; set; }
        public string? Payment { get; set; }
        public int? UserId { get; set; }
        public int? UserAddressId { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? ShipFee { get; set; }
        public int? CouponId { get; set; }
        public int? ShipCouponId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}