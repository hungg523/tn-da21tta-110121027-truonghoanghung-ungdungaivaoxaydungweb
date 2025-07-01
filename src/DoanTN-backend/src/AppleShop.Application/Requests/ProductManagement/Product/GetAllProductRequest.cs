using AppleShop.Share.Abstractions;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Requests.ProductManagement.Product
{
    public class GetAllProductRequest : IQuery<List<Entities.ProductManagement.Product>>
    {
    }
}