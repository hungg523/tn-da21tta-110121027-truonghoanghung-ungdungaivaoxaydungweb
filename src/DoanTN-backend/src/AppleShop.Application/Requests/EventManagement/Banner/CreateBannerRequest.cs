using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.EventManagement.Banner
{
    public class CreateBannerRequest : ICommand
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageData { get; set; }
        public string? Link { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; } = 1;
        public int? Position { get; set; } = 0;
    }
}