using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.PromotionManagement
{
    public class ProductPromotion : BaseEntity
    {
        public int? ProductId { get; set; }
        public int? VariantId { get; set; }
        public int? PromotionId { get; set; }
    }
}