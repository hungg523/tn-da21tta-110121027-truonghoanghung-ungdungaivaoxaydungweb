using AppleShop.Application.Requests.DTOs.UserManagement.Admin;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Admin
{
    public class SearchUserRequest : IQuery<List<ManageUserDTO>>
    {
        public string? Email { get; set; }
    }
}