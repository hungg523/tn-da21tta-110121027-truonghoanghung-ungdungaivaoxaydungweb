using AppleShop.Application.Requests.ReturnManagement.Return;
using AppleShop.Application.Validators.ReturnManagement.Return;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ReturnManagement.Return
{
    public class ChangeStatusReturnHandler : IRequestHandler<ChangeStatusReturnRequest, Result<object>>
    {
        private readonly IReturnRepository returnRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly ICacheService cacheService;

        public ChangeStatusReturnHandler(IReturnRepository returnRepository, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, ICacheService cacheService)
        {
            this.returnRepository = returnRepository;
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(ChangeStatusReturnRequest request, CancellationToken cancellationToken)
        {
            var validator = new ChangeStatusReturnValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var @return = await returnRepository.FindByIdAsync(request.Id, true);
            if (@return is null) AppleException.ThrowNotFound(typeof(Entities.ReturnManagement.Return));

            var orderItem = await orderItemRepository.FindByIdAsync(@return.OiId, true);
            if (orderItem is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.OrderItem));

            var order = await orderRepository.FindByIdAsync(orderItem.OrderId, true);
            if (orderItem is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Order));

            using var transaction = await returnRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.Status == (int)ReturnStatus.Rejected)
                {
                    orderItem.ItemStatus = (int)ItemStatus.RejectedReturn;
                    orderItemRepository.Update(orderItem);
                }

                if (request.Status == (int)ReturnStatus.Approved)
                {
                    orderItem.ItemStatus = (int)ItemStatus.ApprovedReturn;
                    orderItemRepository.Update(orderItem);
                }

                @return.Status = request.Status;
                @return.ProcessedAt = DateTime.Now;
                returnRepository.Update(@return);
                await returnRepository.SaveChangesAsync(cancellationToken);
                await orderItemRepository.SaveChangesAsync(cancellationToken);
                await orderRepository.ExecuteSqlRawAsync("EXEC sp_UpdateOrderStatus @OrderId = {0}", order.Id);
                await cacheService.RemoveByPatternAsync($"return_history_*");
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