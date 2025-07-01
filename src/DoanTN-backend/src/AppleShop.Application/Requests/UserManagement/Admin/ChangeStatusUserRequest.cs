using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.UserManagement.Admin
{
    public class ChangeStatusUserRequest : ICommand<object>
    {
        [JsonIgnore]
        public int? UserId { get; set; }
        public int? IsActived { get; set; }
    }
}