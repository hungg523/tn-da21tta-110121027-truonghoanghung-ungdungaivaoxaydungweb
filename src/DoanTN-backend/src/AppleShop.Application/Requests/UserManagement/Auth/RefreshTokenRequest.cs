using AppleShop.Application.Requests.DTOs.UserManagement.Auth;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class RefreshTokenRequest : ICommand<LoginDTO>
    {
        public string? RefreshToken { get; set; }
    }
}