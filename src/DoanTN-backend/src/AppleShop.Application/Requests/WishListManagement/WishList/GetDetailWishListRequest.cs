using AppleShop.Application.Requests.DTOs.WishListManagement.WishList;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.WishListManagement.WishList
{
    public class GetDetailWishListRequest : IQuery<List<WishListFullDTO>>
    {
        public int? UserId { get; set; }
        public bool? IsActived { get; set; }
    }
}