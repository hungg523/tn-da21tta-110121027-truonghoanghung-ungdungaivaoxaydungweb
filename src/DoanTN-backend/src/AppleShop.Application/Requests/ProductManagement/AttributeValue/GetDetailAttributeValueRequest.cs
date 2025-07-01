using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Requests.ProductManagement.AttributeValue
{
    public class GetDetailAttributeValueRequest : IQuery<Entities.ProductManagement.AttributeValue>
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}