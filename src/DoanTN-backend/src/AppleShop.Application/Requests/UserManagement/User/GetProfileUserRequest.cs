using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.User
{
    public class GetProfileUserRequest : IQuery<UserDTO>
    {
        public int? Id { get; set; }
    }
}