namespace AppleShop.Application.Requests.DTOs.StatisticalManagement.SummaryReport
{
    public class SummaryReportDTO
    {
        public DateTime Date { get; set; }
        public int TotalViews { get; set; }
        public int TotalCartItems { get; set; }
        public int TotalOrders { get; set; }
        public double ViewToCartRate { get; set; } // Tỷ lệ chuyển đổi từ xem -> giỏ hàng
        public double CartToOrderRate { get; set; } // Tỷ lệ chuyển đổi từ giỏ hàng -> đặt hàng
        public double ViewToOrderRate { get; set; } // Tỷ lệ chuyển đổi từ xem -> đặt hàng
        public int TotalCustomers { get; set; }
        public int ReturningCustomers { get; set; } // Số khách hàng quay lại
        public double ReturningCustomerRate { get; set; } // Tỷ lệ khách hàng quay lại
    }
} 