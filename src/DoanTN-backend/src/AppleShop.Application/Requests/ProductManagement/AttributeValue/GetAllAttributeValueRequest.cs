using AppleShop.Share.Abstractions;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Requests.ProductManagement.AttributeValue
{
    public class GetAllAttributeValueRequest : IQuery<List<Entities.ProductManagement.AttributeValue>>
    {
    }
}