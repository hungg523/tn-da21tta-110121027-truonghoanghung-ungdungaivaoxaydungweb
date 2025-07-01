using AppleShop.Application.Requests.DTOs.EventManagement.Banner;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.EventManagement.Banner
{
    public class GetAllBannerRequest : IQuery<List<BannerDTO>>
    {
        public int? UserId { get; set; }
        public int? Status { get; set; }
        public int? Position { get; set; }
    }
}