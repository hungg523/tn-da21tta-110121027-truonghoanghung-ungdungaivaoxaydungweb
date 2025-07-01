using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.UserAddress
{
    public class GetAddressByUserIdRequest : IQuery<List<UserAddressDTO>>
    {
        public int? UserId { get; set; }
    }
}