using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Application.Validators.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Order
{
    public class ChangeOrderStatusHandler : IRequestHandler<ChangeOrderStatusRequest, Result<object>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly ICacheService cacheService;

        public ChangeOrderStatusHandler(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IProductVariantRepository productVariantRepository, ITransactionRepository transactionRepository, ICacheService cacheService)
        {
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.productVariantRepository = productVariantRepository;
            this.transactionRepository = transactionRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(ChangeOrderStatusRequest request, CancellationToken cancellationToken)
        {
            var validator = new ChangeOrderStatusValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var orderItem = await orderItemRepository.FindByIdAsync(request.OiId, true);
            if (orderItem is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.OrderItem));

            var order = await orderRepository.FindByIdAsync(orderItem.OrderId, true);
            if (order is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Order));

            var transactionOrder = await transactionRepository.FindSingleAsync(x => x.OrderId == order.Id, true);
            if (transactionOrder is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Transaction));

            if (orderItem.ItemStatus == (int)ItemStatus.Delivered || orderItem.ItemStatus == (int)ItemStatus.Cancelled) AppleException.ThrowConflict("Can not change status.");

            using var transaction = await orderItemRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.ItemStatus == (int)ItemStatus.Delivered)
                {
                    var productVariant = await productVariantRepository.FindByIdAsync(orderItem.VariantId, true);
                    if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

                    if (transactionOrder.PaymentGateway == "COD")
                    {
                        transactionOrder.Status = (int)TransactionStatus.Success;
                        transactionRepository.Update(transactionOrder);
                        await transactionRepository.SaveChangesAsync(cancellationToken);
                    }

                    productVariant.Stock -= orderItem.Quantity;
                    productVariant.ReservedStock -= orderItem.Quantity;
                    productVariant.SoldQuantity += orderItem.Quantity;
                    productVariantRepository.Update(productVariant);

                    await productVariantRepository.SaveChangesAsync(cancellationToken);
                }

                if (request.ItemStatus == (int)ItemStatus.Cancelled)
                {
                    var productVariant = await productVariantRepository.FindByIdAsync(orderItem.VariantId, true);
                    if (productVariant is null) AppleException.ThrowNotFound(typeof(Entities.ProductManagement.ProductVariant));

                    productVariant.ReservedStock -= orderItem.Quantity;

                    productVariantRepository.Update(productVariant);
                    await productVariantRepository.SaveChangesAsync(cancellationToken);
                }
                orderItem.ItemStatus = request.ItemStatus;
                order.UpdatedAt = DateTime.Now;

                orderRepository.Update(order);
                orderItemRepository.Update(orderItem);
                await orderRepository.SaveChangesAsync(cancellationToken);
                await orderItemRepository.SaveChangesAsync(cancellationToken);
                await orderRepository.ExecuteSqlRawAsync("EXEC sp_UpdateOrderStatus @OrderId = {0}", order.Id);

                transaction.Commit();
                await cacheService.RemoveByPatternAsync("product_variants_*");
                await cacheService.RemoveAsync($"product_detail_{orderItem.VariantId}");
                await cacheService.RemoveByPatternAsync($"purchase_history_*");
                await cacheService.RemoveByPatternAsync($"all_orders_*");
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