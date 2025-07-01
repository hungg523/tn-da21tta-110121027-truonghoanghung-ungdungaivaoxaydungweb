using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class RegisterRequest : ICommand
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}