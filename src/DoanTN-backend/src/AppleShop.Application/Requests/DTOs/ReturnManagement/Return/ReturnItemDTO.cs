namespace AppleShop.Application.Requests.DTOs.ReturnManagement.Return
{
    public class ReturnItemDTO
    {
        public int? ReturnId { get; set; }
        public int? VariantId { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? ProductAttribute { get; set; }
        public string? ImageUrl { get; set; }
        public int? Quantity { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}