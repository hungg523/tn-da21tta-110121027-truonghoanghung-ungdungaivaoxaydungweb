using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.EventManagement.Banner
{
    public class UpdateBannerRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageData { get; set; }
        public string? Link { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public int? Position { get; set; }
    }
}