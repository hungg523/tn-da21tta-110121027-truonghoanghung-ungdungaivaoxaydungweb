using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class VerifyOTPRequest : ICommand
    {
        public string? Email { get; set; }
        public string? OTP { get; set; }
    }
}