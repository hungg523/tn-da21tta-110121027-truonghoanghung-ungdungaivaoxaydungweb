using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.UserManagement.User
{
    public class UpdateProfileUserRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public string? UserName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImageData { get; set; }
    }
}