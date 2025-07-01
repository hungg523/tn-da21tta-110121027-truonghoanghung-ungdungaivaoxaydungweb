using AppleShop.Share.Abstractions;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Requests.ProductManagement.Product
{
    public class GetProductDetailRequest : IQuery<Entities.ProductManagement.Product>
    {
        public int? Id { get; set; }
    }
}