using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.ProductImage
{
    public class CreateProductImageRequest : ICommand
    {
        public string? Title { get; set; }
        public string? ImageData { get; set; }
        public int? Position { get; set; } = 0;
        
        [JsonIgnore]
        public int? ProductId { get; set; }
        public int? VariantId { get; set; }
    }
}