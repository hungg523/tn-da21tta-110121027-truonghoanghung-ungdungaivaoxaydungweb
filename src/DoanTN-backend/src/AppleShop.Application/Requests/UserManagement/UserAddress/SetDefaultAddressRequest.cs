using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.UserManagement.UserAddress
{
    public class SetDefaultAddressRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? UserId { get; set; }
    }
}