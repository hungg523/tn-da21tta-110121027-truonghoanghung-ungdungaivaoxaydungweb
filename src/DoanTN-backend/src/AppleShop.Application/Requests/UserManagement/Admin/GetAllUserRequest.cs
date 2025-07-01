using AppleShop.Application.Requests.DTOs.UserManagement.Admin;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.Admin
{
    public class GetAllUserRequest : IQuery<ManageUserListResponseDTO>
    {
        public int? Role { get; set; }
        public int? IsActived { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}