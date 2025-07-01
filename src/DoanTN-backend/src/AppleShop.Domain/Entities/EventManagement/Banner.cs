using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.EventManagement
{
    public class Banner : BaseEntity
    {
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