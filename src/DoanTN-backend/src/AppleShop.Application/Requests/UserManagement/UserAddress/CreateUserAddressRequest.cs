using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.UserManagement.UserAddress
{
    public class CreateUserAddressRequest : ICommand
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AddressLine { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }

        [JsonIgnore]
        public int? UserId { get; set; }

        [JsonIgnore]
        public bool? Default { get; set; } = false;
    }
}