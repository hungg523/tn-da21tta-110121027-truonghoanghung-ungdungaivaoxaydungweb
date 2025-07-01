namespace AppleShop.Application.Requests.DTOs.ProductManagement.Product
{
    public class ProductDetailDTO
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public List<DetailDTO> Detail { get; set; } = new List<DetailDTO>();
    }
}