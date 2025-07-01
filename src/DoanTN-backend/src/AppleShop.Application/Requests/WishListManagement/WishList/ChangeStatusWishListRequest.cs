using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.WishListManagement.WishList
{
    public class ChangeStatusWishListRequest : ICommand
    {
        [JsonIgnore]
        public int? VariantId { get; set; }

        [JsonIgnore]
        public int? UserId { get; set; }
        public bool? IsActived { get; set; }
    }
}