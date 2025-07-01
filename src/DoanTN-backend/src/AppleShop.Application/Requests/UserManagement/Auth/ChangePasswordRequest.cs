using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.UserManagement.Auth
{
    public class ChangePasswordRequest : ICommand
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}