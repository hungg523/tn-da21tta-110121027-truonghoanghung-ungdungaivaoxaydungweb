using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class LogoutRequest : ICommand
    {
        public int? UserId { get; set; }
    }
}