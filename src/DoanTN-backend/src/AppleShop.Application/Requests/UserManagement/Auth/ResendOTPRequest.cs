using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class ResendOTPRequest : ICommand
    {
        public string? Email { get; set; }
    }
}