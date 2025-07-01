using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.PromotionManagement.CouponType
{
    public class DeleteCouponTypeRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}