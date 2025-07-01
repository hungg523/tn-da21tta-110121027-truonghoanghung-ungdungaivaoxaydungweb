using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Application.Validators.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using Entities = AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MassTransit;
using MediatR;

namespace AppleShop.Application.Handlers.OrderManagement.Order
{
    public class GetAllOrderHandler : IRequestHandler<GetAllOrderRequest, Result<OrderListResponseDTO>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IUserRepository userRepository;
        private readonly IUserAddressRepository userAddressRepository;
        private readonly IPayOSService payOSService;
        private readonly ICacheService cacheService;

        public GetAllOrderHandler(IOrderRepository orderRepository,
                                  IOrderItemRepository orderItemRepository,
                                  IUserRepository userRepository,
                                  IUserAddressRepository userAddressRepository,
                                  IPayOSService payOSService,
                                  ICacheService cacheService)
        {
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.userRepository = userRepository;
            this.userAddressRepository = userAddressRepository;
            this.payOSService = payOSService;
            this.cacheService = cacheService;
        }

        public async Task<Result<OrderListResponseDTO>> Handle(GetAllOrderRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetAllOrderValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var cacheKey = $"all_orders_{request.Status}_{request.Skip}_{request.Take}";
            var cachedResult = await cacheService.GetAsync<Result<OrderListResponseDTO>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            List<Entities.Order> orders = new List<Entities.Order>();
            if (request.Status is not null) orders = orderRepository.FindAll(x => x.Status == request.Status, false, o => o.OrderItems).ToList();
            else orders = orderRepository.FindAll(null, false, o => o.OrderItems).ToList();

            var users = userRepository.FindAll(x => orders.Select(x => x.UserId).Contains(x.Id)).ToDictionary(u => u.Id, u => u.Email);
            var userAddresses = userAddressRepository.FindAll(x => orders.Select(x => x.UserAddressId).Contains(x.Id)).ToList();

            var orderDtos = orders.Select(order => new OrderDTO
            {
                OrderId = order.Id,
                Code = order.OrderCode,
                Email = order.UserId.HasValue && users.ContainsKey(order.UserId.Value) ? users[order.UserId] : null,
                Status = ((OrderStatus)order.Status).ToString(),
                TotalQuantity = orderItemRepository.FindAll(x => x.OrderId == order.Id, false).Count(),
                TotalAmount = order.TotalAmount ?? 0
            }).OrderByDescending(x => x.OrderId).ToList();

            var totalItems = orderDtos.Count();
            var pagedDtos = orderDtos.Skip(request.Skip ?? 0).Take(request.Take ?? totalItems).ToList();
            var result = Result<OrderListResponseDTO>.Ok(new OrderListResponseDTO { TotalItems = totalItems, Items = pagedDtos });

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
    }
}