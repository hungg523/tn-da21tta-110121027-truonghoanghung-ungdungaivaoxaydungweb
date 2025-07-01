using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.PromotionManagement.ProductPromotion
{
    public class CreateProductPromotionRequest : ICommand
    {
        public ICollection<int>? ProductIds { get; set; } = new List<int>();
        public ICollection<int>? VariantIds { get; set; } = new List<int>();
        public int? PromotionId { get; set; }
    }
}