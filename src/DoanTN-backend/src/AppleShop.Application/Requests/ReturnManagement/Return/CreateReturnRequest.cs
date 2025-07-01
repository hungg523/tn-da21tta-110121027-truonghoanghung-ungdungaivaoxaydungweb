using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReturnManagement.Return
{
    public class CreateReturnRequest : ICommand
    {
        public int? OrderItemId { get; set; }
        
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? Quantity { get; set; }
        public string? Reason { get; set; }
        public string? ImageData { get; set; }

        [JsonIgnore]
        public int? Status { get; set; } = (int)ReturnStatus.Pending;

        [JsonIgnore]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public string? ReturnType { get; set; } = "COD";
    }
}