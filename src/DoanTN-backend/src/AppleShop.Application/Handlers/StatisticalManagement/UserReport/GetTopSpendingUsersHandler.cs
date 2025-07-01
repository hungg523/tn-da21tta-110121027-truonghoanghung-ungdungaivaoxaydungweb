using AppleShop.Application.Requests.DTOs.StatisticalManagement.UserReport;
using AppleShop.Application.Requests.StatisticalManagement.UserReport;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppleShop.Application.Handlers.StatisticalManagement.UserReport
{
    public class GetTopSpendingUsersHandler : IRequestHandler<GetTopSpendingUsersRequest, Result<List<TopSpendingUserDTO>>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;
        private readonly ICacheService cacheService;

        public GetTopSpendingUsersHandler(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IFileService fileService,
            ICacheService cacheService)
        {
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.fileService = fileService;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<TopSpendingUserDTO>>> Handle(GetTopSpendingUsersRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"top_spending_users_{request.StartDate}_{request.EndDate}_{request.TopCount}";
            var cachedResult = await cacheService.GetAsync<Result<List<TopSpendingUserDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var query = orderRepository.FindAll(includeProperties: o => o.OrderItems).Where(o => o.OrderItems.Any(i => i.ItemStatus == (int)ItemStatus.Delivered));

            if (request.StartDate.HasValue) query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.CreatedAt <= request.EndDate.Value);

            var orders = await query.ToListAsync(cancellationToken);
            var userIds = orders.Select(o => o.UserId).Where(id => id.HasValue).Distinct().ToList();

            var users = await userRepository.FindAll(x => userIds.Contains(x.Id)).ToDictionaryAsync(u => u.Id, u => u, cancellationToken);

            var userSpending = orders
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSpending = g.Sum(o => o.TotalAmount ?? 0),
                    TotalOrders = g.Count()
                }).OrderByDescending(x => x.TotalSpending).Take(request.TopCount ?? 5).ToList();

            var result = userSpending.Select(x => new TopSpendingUserDTO
            {
                UserId = x.UserId,
                Email = x.UserId.HasValue && users.ContainsKey(x.UserId.Value) ? users[x.UserId.Value].Email : null,
                Avatar = x.UserId.HasValue && users.ContainsKey(x.UserId.Value) ? users[x.UserId.Value].ImageUrl.StartsWith("https") ? users[x.UserId.Value].ImageUrl : fileService.GetFullPathFileServer(users[x.UserId.Value].ImageUrl) : null,
                TotalSpending = (double?)x.TotalSpending,
                TotalOrders = x.TotalOrders,
                AverageOrderValue = x.TotalOrders > 0 ? (double?)(x.TotalSpending / x.TotalOrders) : 0
            }).ToList();

            var finalResult = Result<List<TopSpendingUserDTO>>.Ok(result);

            await cacheService.SetAsync(cacheKey, finalResult, TimeSpan.FromMinutes(5));

            return finalResult;
        }
    }
}