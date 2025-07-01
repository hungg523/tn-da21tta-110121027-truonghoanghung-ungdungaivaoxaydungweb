//using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
//using Entities = AppleShop.Domain.Entities.OrderManagement;
//using AppleShop.Share.Abstractions;
//using AppleShop.Share.Enumerations;

//namespace AppleShop.Infrastructure.BackgroundJob
//{
//    public class OrderCleanupJob
//    {
//        private readonly IOrderRepository orderRepository;
//        private readonly IPaymentRepository paymentRepository;
//        private readonly ITransactionRepository transactionRepository;
//        private readonly IOrderItemRepository orderItemRepository;
//        private readonly IPayOSService payOSService;

//        public OrderCleanupJob(IOrderRepository orderRepository,
//                               IPaymentRepository paymentRepository,
//                               ITransactionRepository transactionRepository,
//                               IOrderItemRepository orderItemRepository,
//                               IPayOSService payOSService)
//        {
//            this.orderRepository = orderRepository;
//            this.paymentRepository = paymentRepository;
//            this.transactionRepository = transactionRepository;
//            this.orderItemRepository = orderItemRepository;
//            this.payOSService = payOSService;
//        }

//        public async Task CancelUnpaidOrdersAsync()
//        {
//            var thirtyMinutesAgo = DateTime.Now.AddMinutes(-30);
//            var pendingOrders = orderRepository.FindAll(o => o.Status == (int)OrderStatus.Pending && o.Payment.ToLower() == Payment.PayOS .ToString().ToLower() && o.CreatedAt < thirtyMinutesAgo, true, oi => oi.OrderItems).ToList();
//            var cancelledOrders = new List<Entities.Order>();
//            var cancelledOrderItems = new List<Entities.OrderItem>();
//            var failedPayments = new List<Entities.Payment>();
//            var failedTransactions = new List<Entities.Transaction>();

//            foreach (var order in pendingOrders)
//            {
//                var response = await payOSService.GetOrderPayOSAsync(order.OrderCode);
//                if (response.status.ToLower() == OrderStatus.Pending.ToString().ToLower())
//                {
//                    order.Status = (int)OrderStatus.Cancelled;
//                    cancelledOrders.Add(order);

//                    foreach (var item in order.OrderItems)
//                    {
//                        item.ItemStatus = (int)ItemStatus.Cancelled;
//                        cancelledOrderItems.Add(item);
//                    }

//                    var payment = await paymentRepository.FindSingleAsync(p => p.OrderId == order.Id, true);
//                    if (payment is not null)
//                    {
//                        payment.Status = (int)TransactionStatus.Failed;
//                        failedPayments.Add(payment);
//                    }

//                    var transaction = await transactionRepository.FindSingleAsync(t => t.OrderId == order.Id, true);
//                    if (transaction is not null)
//                    {
//                        transaction.Status = (int)TransactionStatus.Failed;
//                        failedTransactions.Add(transaction);
//                    }

//                    await payOSService.CancelPaymentAsync(order.OrderCode);
//                }
//            }

//            orderRepository.UpdateRange(cancelledOrders);
//            orderItemRepository.UpdateRange(cancelledOrderItems);
//            paymentRepository.UpdateRange(failedPayments);
//            transactionRepository.UpdateRange(failedTransactions);

//            await orderRepository.SaveChangesAsync();
//            await orderItemRepository.SaveChangesAsync();
//            await paymentRepository.SaveChangesAsync();
//            await transactionRepository.SaveChangesAsync();
//        }
//    }
//}