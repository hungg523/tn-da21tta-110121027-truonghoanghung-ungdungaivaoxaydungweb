using AppleShop.Domain.Abstractions.Common;
using System.Text.Json.Serialization;

namespace AppleShop.Domain.Entities.ProductManagement
{
    public class Attribute : BaseEntity
    {
        public string? Name { get; set; }
    }
}