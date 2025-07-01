using AppleShop.Application.Requests.DTOs.OrderManagement.Saga;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Sagas
{
    public class ProcessPaymentStep : ISagaStep
    {
        private readonly IOrderRepository orderRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly CreateOrderRequest request;
        private readonly OrderSagaDTO orderSagaDTO;

        private Entities.OrderManagement.Order? rollbackOrder;
        private Transaction? rollbackTransaction;

        public ProcessPaymentStep(IOrderRepository orderRepository, ITransactionRepository transactionRepository, CreateOrderRequest request, OrderSagaDTO orderSagaDTO)
        {
            this.orderRepository = orderRepository;
            this.transactionRepository = transactionRepository;
            this.request = request;
            this.orderSagaDTO = orderSagaDTO;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var order = await orderRepository.FindSingleAsync(x => x.OrderCode == orderSagaDTO.Code, true);

            var transaction = new Transaction
            {
                OrderId = order.Id,
                PaymentGateway = request.Payment,
                Code = order.OrderCode,
                Amount = order.TotalAmount,
                Status = (int)TransactionStatus.Pending,
                CreatedAt = DateTime.Now
            };

            transactionRepository.Create(transaction);
            await transactionRepository.SaveChangesAsync(cancellationToken);

            rollbackOrder = order;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            rollbackTransaction = await transactionRepository.FindSingleAsync(x => x.OrderId == rollbackOrder.Id, true);
            if (rollbackTransaction is null) return;

            rollbackTransaction.Status = (int)TransactionStatus.Failed;
        }
    }
}