using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class VerifyResetPasswordRequest : ICommand
    {
        public string? Email { get; set; }
    }
}