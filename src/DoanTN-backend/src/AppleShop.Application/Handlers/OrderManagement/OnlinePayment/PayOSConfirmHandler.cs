using AppleShop.Application.Requests.OrderManagement.OnlinePayment;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.OnlinePayment
{
    public class PayOSConfirmHandler : IRequestHandler<PayOSConfirmRequest, Result<object>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IPayOSService payOS;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IProductVariantRepository productVariantRepository;

        public PayOSConfirmHandler(IOrderRepository orderRepository, IPaymentRepository paymentRepository, ITransactionRepository transactionRepository, IPayOSService payOS, IOrderItemRepository orderItemRepository, IProductVariantRepository productVariantRepository)
        {
            this.orderRepository = orderRepository;
            this.paymentRepository = paymentRepository;
            this.transactionRepository = transactionRepository;
            this.payOS = payOS;
            this.orderItemRepository = orderItemRepository;
            this.productVariantRepository = productVariantRepository;
        }

        public async Task<Result<object>> Handle(PayOSConfirmRequest request, CancellationToken cancellationToken)
        {
            var response = await payOS.GetOrderPayOSAsync(request.OrderCode);
            if (response is null) AppleException.ThrowNotFound(message: "Order Code is not found");

            var order = await orderRepository.FindSingleAsync(x => x.OrderCode == request.OrderCode.ToString(), true, cancellationToken, oi => oi.OrderItems);
            if (order is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Order));

            var payment = await paymentRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
            if (payment is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Payment));

            var transactionOrder = await transactionRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
            if (transactionOrder is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Transaction));

            using var transaction = await orderRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var transactionCode = $"PAYOS-{request.OrderCode}-{DateTime.Now:yyyyMMddHHmmss}";

                if (response.status.ToLower() == OrderStatus.Paid.ToString().ToLower())
                {
                    order.Status = (int)OrderStatus.Paid;
                    order.UpdatedAt = DateTime.Now;
                    orderRepository.Update(order);

                    foreach (var item in order.OrderItems)
                    {
                        item.ItemStatus = (int)ItemStatus.Packed;
                        orderItemRepository.Update(item);
                    }

                    transactionOrder.Status = (int)TransactionStatus.Success;
                    transactionOrder.Code = transactionCode;
                    transactionOrder.UpdatedAt = DateTime.Now;
                    transactionRepository.Update(transactionOrder);

                    payment.Status = (int)PaymentStatus.Success;
                    payment.TransactionCode = transactionCode;
                    payment.UpdatedAt = DateTime.Now;
                    paymentRepository.Update(payment);
                }
                
                if (response.status.ToLower() == OrderStatus.Cancelled.ToString().ToLower())
                {
                    order.Status = (int)OrderStatus.Cancelled;
                    order.UpdatedAt = DateTime.Now;
                    orderRepository.Update(order);

                    foreach(var item in order.OrderItems)
                    {
                        item.ItemStatus = (int)ItemStatus.Cancelled;
                        var variant = await productVariantRepository.FindByIdAsync(item.VariantId, true);
                        variant.ReservedStock -= item.Quantity;

                        productVariantRepository.Update(variant);
                        orderItemRepository.Update(item);
                    }

                    transactionOrder.Status = (int)TransactionStatus.Failed;
                    transactionOrder.Code = transactionCode;
                    transactionOrder.UpdatedAt = DateTime.Now;
                    transactionRepository.Update(transactionOrder);

                    payment.Status = (int)PaymentStatus.Failed;
                    payment.TransactionCode = transactionCode;
                    payment.UpdatedAt = DateTime.Now;
                    paymentRepository.Update(payment);

                    await productVariantRepository.SaveChangesAsync(cancellationToken);
                }

                await orderRepository.SaveChangesAsync(cancellationToken);
                await orderItemRepository.SaveChangesAsync(cancellationToken);
                await transactionRepository.SaveChangesAsync(cancellationToken);
                await paymentRepository.SaveChangesAsync(cancellationToken);

                transaction.Commit();
                return Result<object>.Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}