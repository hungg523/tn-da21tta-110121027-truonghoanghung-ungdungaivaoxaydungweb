namespace AppleShop.Application.Requests.DTOs.StatisticalManagement.ReturnReport
{
    public class ReturnReportDTO
    {
        public DateTime Date { get; set; }
        public int TotalReturns { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public List<ReturnReasonDTO> TopReasons { get; set; } = new();
    }

    public class ReturnReasonDTO
    {
        public string Reason { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}