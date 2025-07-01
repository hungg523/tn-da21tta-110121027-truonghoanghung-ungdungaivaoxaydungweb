using AppleShop.Application.Requests.DTOs.UserManagement.Auth;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class LoginRequest : ICommand<LoginDTO>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}