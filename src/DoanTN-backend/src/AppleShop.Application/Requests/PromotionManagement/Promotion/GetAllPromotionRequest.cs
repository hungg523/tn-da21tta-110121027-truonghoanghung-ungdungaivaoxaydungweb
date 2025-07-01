using AppleShop.Application.Requests.DTOs.PromotionManagement.ProductPromotion;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.PromotionManagement.Promotion
{
    public class GetAllPromotionRequest : IQuery<List<PromotionDTO>>
    {
        public int? IsActived { get; set; }
    }
}