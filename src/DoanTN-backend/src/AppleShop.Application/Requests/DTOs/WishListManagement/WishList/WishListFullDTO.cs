namespace AppleShop.Application.Requests.DTOs.WishListManagement.WishList
{
    public class WishListFullDTO
    {
        public int? Id { get; set; }
        public int? VariantId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public List<string>? ProductAttributes { get; set; }
    }
}