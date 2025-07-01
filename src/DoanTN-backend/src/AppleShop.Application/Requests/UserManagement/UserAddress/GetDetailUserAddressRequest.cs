using AppleShop.Share.Abstractions;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Requests.UserManagement.UserAddress
{
    public class GetDetailUserAddressRequest : IQuery<Entities.UserManagement.UserAddress>
    {
        public int? Id { get; set; }
    }
}