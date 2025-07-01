using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.DTOs.UserManagement.User
{
    public class UserAddressDTO
    {
        public int? AddressId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FullName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PhoneNumber { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Address { get; set; }
    }
}