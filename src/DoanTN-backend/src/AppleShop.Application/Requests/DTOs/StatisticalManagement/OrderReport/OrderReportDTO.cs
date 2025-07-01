namespace AppleShop.Application.Requests.DTOs.StatisticalManagement.OrderReport
{
    public class OrderReportDTO
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int SuccessfulOrders { get; set; }
        public int FailedOrders { get; set; }
        public double SuccessRate { get; set; }
    }
}