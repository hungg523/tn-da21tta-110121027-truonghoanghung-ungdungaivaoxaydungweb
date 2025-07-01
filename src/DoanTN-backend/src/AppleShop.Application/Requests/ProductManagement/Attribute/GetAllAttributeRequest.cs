using AppleShop.Share.Abstractions;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Requests.ProductManagement.Attribute
{
    public class GetAllAttributeRequest : IQuery<List<Entities.ProductManagement.Attribute>>
    {
    }
}