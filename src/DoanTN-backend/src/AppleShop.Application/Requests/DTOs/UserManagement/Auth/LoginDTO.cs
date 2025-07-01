namespace AppleShop.Application.Requests.DTOs.UserManagement.Auth
{
    public class LoginDTO
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}