namespace AppleShop.Application.Requests.DTOs.StatisticalManagement.UserReport
{
    public class TopSpendingUserDTO
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public double? TotalSpending { get; set; }
        public int? TotalOrders { get; set; }
        public double? AverageOrderValue { get; set; }
    }
} 