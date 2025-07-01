using Entities = AppleShop.Domain.Entities;
using AppleShop.Share.Abstractions;
using AppleShop.Application.Requests.DTOs.PromotionManagement.ProductPromotion;

namespace AppleShop.Application.Requests.PromotionManagement.ProductPromotion
{
    public class GetAllProductPromotionRequest : IQuery<List<ProductPromotionDTO>>
    {
    }
}