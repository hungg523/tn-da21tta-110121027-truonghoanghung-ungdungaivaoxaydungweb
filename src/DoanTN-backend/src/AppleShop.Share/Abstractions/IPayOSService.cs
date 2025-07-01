using AppleShop.Share.Shared;
using Net.payOS.Types;

namespace AppleShop.Share.Abstractions
{
    public interface IPayOSService
    {
        Task<string> CreatePaymentUrlAsync(string orderCodeStr, decimal amount, List<ItemData> items);
        Task<PaymentLinkInformation> GetOrderPayOSAsync(string? orderCode);
        Task<string> GetPaymentUrlAsync(string? orderCode);
        Task<PayOSDTO> GetPaymentStatusAsync(string? orderCode);
        Task<string> CancelPaymentAsync(string? orderCode, string? reason = null);
        bool ConfirmWebhook(string? webhookUrl);
    }
}