using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.UserManagement.UserCoupon
{
    public class CreateUserCouponRequest : ICommand
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? CouponId { get; set; }
    }
}