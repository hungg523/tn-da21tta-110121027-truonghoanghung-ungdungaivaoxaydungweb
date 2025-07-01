using AppleShop.Share.Abstractions;
using AppleShop.Share.Constant;
using AppleShop.Share.Shared;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Cryptography;
using System.Text;

namespace AppleShop.Share.Service
{
    public class PayOSService : IPayOSService
    {
        private readonly IConfiguration config;
        private readonly PayOS payOS;

        public PayOSService(IConfiguration config)
        {
            this.config = config;
            payOS = new PayOS(
                this.config[Const.PAYOS_CLIENT_ID]!,
                this.config[Const.PAYOS_API_KEY]!,
                this.config[Const.PAYOS_CHECKSUM_KEY]!
            );
        }

        public async Task<PaymentLinkInformation> GetOrderPayOSAsync(string? orderCode)
        {
            PaymentLinkInformation paymentLinkInformation = await payOS.getPaymentLinkInformation(long.Parse(orderCode));
            return paymentLinkInformation;
        }

        public async Task<string> GetPaymentUrlAsync(string? orderCode)
        {
            PaymentLinkInformation paymentLinkInformation = await payOS.getPaymentLinkInformation(long.Parse(orderCode));
            return paymentLinkInformation.id;
        }

        public async Task<PayOSDTO> GetPaymentStatusAsync(string? orderCode)
        {
            PaymentLinkInformation paymentLinkInformation = await payOS.getPaymentLinkInformation(long.Parse(orderCode));
            return new PayOSDTO 
            {
                Id = paymentLinkInformation.id,
                Status = paymentLinkInformation.status
            };
        }

        public async Task<string> CreatePaymentUrlAsync(string orderCodeStr, decimal amount, List<ItemData> items)
        {
            var orderCode = long.Parse(orderCodeStr);
            var intAmount = (int)amount;
            var cancelUrl = config[Const.PAYOS_CANCEL_URL];
            var returnUrl = config[Const.PAYOS_RETURN_URL];
            var checksumKey = config["PayOS:ChecksumKey"];
            var description = $"Thanh toan don hang";

            var signature = GenerateSignature(checksumKey, orderCode, intAmount, description, cancelUrl, returnUrl);

            var paymentData = new PaymentData(
                orderCode,
                2000, //intAmount,
                description,
                items,
                cancelUrl,
                returnUrl,
                signature,
                expiredAt: (long)DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
            );

            var result = await payOS.createPaymentLink(paymentData);
            return result.checkoutUrl;
        }

        public static string GenerateSignature(string key, long orderCode, int amount, string description, string cancelUrl, string returnUrl)
        {
            var rawData = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public async Task<string> CancelPaymentAsync(string? orderCode, string? reason = null)
        {
            PaymentLinkInformation cancelledPaymentLinkInfo = await payOS.cancelPaymentLink(long.Parse(orderCode), reason ?? "Transaction deadline.");
            
            var cancelUrl = config[Const.PAYOS_CANCEL_URL];
            return cancelUrl;
        }

        public bool ConfirmWebhook(string? webhookUrl)
        {
            payOS.confirmWebhook(webhookUrl);
            return true;
        }
    }
}