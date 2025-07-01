using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class ResetPasswordRequest : ICommand
    {
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }
}