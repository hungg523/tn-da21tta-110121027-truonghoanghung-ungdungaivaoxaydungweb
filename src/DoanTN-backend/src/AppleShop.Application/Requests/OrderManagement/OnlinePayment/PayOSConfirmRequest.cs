using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.OrderManagement.OnlinePayment
{
    public class PayOSConfirmRequest : ICommand
    {
        public string? OrderCode { get; set; }
    }
}