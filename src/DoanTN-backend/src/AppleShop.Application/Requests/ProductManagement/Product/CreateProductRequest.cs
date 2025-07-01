using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.Product
{
    public class CreateProductRequest : ICommand
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [JsonIgnore]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public int? CategoryId { get; set; }
        public int? IsActived { get; set; } = 0;
    }
}