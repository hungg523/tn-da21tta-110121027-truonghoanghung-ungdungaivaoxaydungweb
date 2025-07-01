using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class Product : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CategoryId { get; set; }
        public int? IsActived { get; set; }
    }
}