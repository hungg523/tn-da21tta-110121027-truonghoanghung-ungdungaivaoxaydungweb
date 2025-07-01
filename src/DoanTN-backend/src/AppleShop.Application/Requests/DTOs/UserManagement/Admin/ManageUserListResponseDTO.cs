using System.Collections.Generic;

namespace AppleShop.Application.Requests.DTOs.UserManagement.Admin
{
    public class ManageUserListResponseDTO
    {
        public int TotalItems { get; set; }
        public List<ManageUserDTO> Items { get; set; } = new List<ManageUserDTO>();
    }
} 