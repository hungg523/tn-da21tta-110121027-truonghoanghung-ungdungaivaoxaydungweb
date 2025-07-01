namespace AppleShop.Application.Requests.DTOs.UserManagement.Admin
{
    public class ManageUserDTO
    {
        public int? Id { get; set; }
        public string? Avatar { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? IsActived { get; set; }
        public int? TotalOrders { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}