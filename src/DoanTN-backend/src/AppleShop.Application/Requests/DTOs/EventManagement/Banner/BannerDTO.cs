namespace AppleShop.Application.Requests.DTOs.EventManagement.Banner
{
    public class BannerDTO
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Link { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public int? Position { get; set; }
    }
}