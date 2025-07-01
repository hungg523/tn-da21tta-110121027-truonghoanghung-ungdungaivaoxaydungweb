using AppleShop.Application.Requests.ReturnManagement.Return;
using AppleShop.Application.Services;
using AppleShop.Application.Validators.ReturnManagement.Return;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.ReturnManagement.Return
{
    public class CreateReturnHandler : IRequestHandler<CreateReturnRequest, Result<object>>
    {
        private readonly IReturnRepository returnRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;
        private readonly INotificationService notificationService;
        private readonly ICacheService cacheService;

        public CreateReturnHandler(IReturnRepository returnRepository, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IMapper mapper, IUserRepository userRepository, IFileService fileService, INotificationService notificationService, ICacheService cacheService)
        {
            this.returnRepository = returnRepository;
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.fileService = fileService;
            this.notificationService = notificationService;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateReturnRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateReturnValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var user = await userRepository.FindByIdAsync(request.UserId);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            var orderItem = await orderItemRepository.FindByIdAsync(request.OrderItemId, true);
            if (orderItem is null) AppleException.ThrowNotFound(typeof(OrderItem));

            if (request.Quantity > orderItem.Quantity) AppleException.ThrowConflict("Invalid return quantity.");

            var order = await orderRepository.FindByIdAsync(orderItem.OrderId);
            if (order!.Status != (int)OrderStatus.Successed || !order.UpdatedAt.HasValue || order.UpdatedAt.Value.AddDays(7) < DateTime.Now) AppleException.ThrowConflict("Returns are not allowed if the order is more than 7 days old or has not been confirmed as success.");

            using var transaction = await returnRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var quantity = orderItem.Quantity == 1 ? orderItem.Quantity : request.Quantity;
                var @return = mapper.Map<Entities.ReturnManagement.Return>(request);
                @return.OiId = orderItem.Id;
                @return.Quantity = quantity;
                @return.RefundAmount = orderItem.TotalPrice;
                returnRepository.Create(@return);
                await returnRepository.SaveChangesAsync(cancellationToken);
                if (request.ImageData is not null)
                {
                    var uploadFile = new UploadFileRequest
                    {
                        Content = request.ImageData,
                        AssetType = AssetType.Return,
                        Suffix = @return.Id.ToString()
                    };
                    user.ImageUrl = await fileService.UploadFileAsync(uploadFile);
                }

                await notificationService.NotifyNewReturnRequest(@return.Id.Value, order.OrderCode);
                transaction.Commit();
                await cacheService.RemoveByPatternAsync($"return_history_*");
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