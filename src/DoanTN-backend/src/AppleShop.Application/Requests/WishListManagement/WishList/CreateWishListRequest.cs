using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.WishListManagement.WishList
{
    public class CreateWishListRequest : ICommand
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? VariantId { get; set; }

        [JsonIgnore]
        public DateTime? AddedDate { get; set; } = DateTime.Now;

        [JsonIgnore]
        public bool? IsActived { get; set; } = true;
    }
}