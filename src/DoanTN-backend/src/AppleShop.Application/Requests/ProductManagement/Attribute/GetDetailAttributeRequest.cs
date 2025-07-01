using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Requests.ProductManagement.Attribute
{
    public class GetDetailAttributeRequest : IQuery<Entities.ProductManagement.Attribute>
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}