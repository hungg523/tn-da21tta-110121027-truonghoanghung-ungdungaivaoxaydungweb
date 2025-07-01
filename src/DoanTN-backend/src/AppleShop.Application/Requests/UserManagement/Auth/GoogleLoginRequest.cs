using AppleShop.Application.Requests.DTOs.UserManagement.Auth;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class GoogleLoginRequest : ICommand<LoginDTO>
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? ImageUrl { get; set; }
    }
}