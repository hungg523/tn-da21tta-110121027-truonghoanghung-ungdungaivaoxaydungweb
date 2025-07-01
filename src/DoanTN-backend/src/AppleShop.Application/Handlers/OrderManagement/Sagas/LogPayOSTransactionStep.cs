using AppleShop.Application.Requests.DTOs.OrderManagement.Saga;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Sagas
{
    public class LogPayOSTransactionStep : ISagaStep
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IOrderRepository orderRepository;
        private readonly CreateOrderRequest request;
        private readonly OrderSagaDTO orderSagaDTO;

        public LogPayOSTransactionStep(IPaymentRepository paymentRepository, ITransactionRepository transactionRepository, IOrderRepository orderRepository, CreateOrderRequest request, OrderSagaDTO orderSagaDTO)
        {
            this.paymentRepository = paymentRepository;
            this.transactionRepository = transactionRepository;
            this.orderRepository = orderRepository;
            this.request = request;
            this.orderSagaDTO = orderSagaDTO;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (request.Payment.ToLower() != Payment.PayOS.ToString().ToLower()) return;

            var order = await orderRepository.FindSingleAsync(x => x.OrderCode == orderSagaDTO.Code, false);
            var payment = new Entities.OrderManagement.Payment
            {
                OrderId = order.Id,
                PaymentMethod = (int)Payment.PayOS,
                Amount = order.TotalAmount,
                Status = (int)PaymentStatus.Pending,
                TransactionCode = order.OrderCode,
                CreatedAt = DateTime.Now
            };

            paymentRepository.Create(payment);

            var transaction = await transactionRepository.FindSingleAsync(x => x.OrderId == order.Id, true);

            transaction.PaymentGateway = Payment.PayOS.ToString();
            transaction.UpdatedAt = DateTime.Now;

            transactionRepository.Update(transaction);

            await paymentRepository.SaveChangesAsync(cancellationToken);
            await transactionRepository.SaveChangesAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}