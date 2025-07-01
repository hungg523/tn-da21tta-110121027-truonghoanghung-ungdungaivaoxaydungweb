using AppleShop.Application.Requests.DTOs.StatisticalManagement.OrderReport;
using AppleShop.Application.Requests.StatisticalManagement.OrderReport;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppleShop.Application.Handlers.StatisticalManagement.OrderReport
{
    public class GetOrderReportHandler : IRequestHandler<GetOrderReportRequest, Result<List<OrderReportDTO>>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly ICacheService cacheService;

        public GetOrderReportHandler(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            ICacheService cacheService)
        {
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<OrderReportDTO>>> Handle(GetOrderReportRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"order_report_{request.StartDate}_{request.EndDate}_{request.TimeUnit}";
            var cachedResult = await cacheService.GetAsync<Result<List<OrderReportDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var query = orderRepository.FindAll();

            if (request.StartDate.HasValue) query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.CreatedAt <= request.EndDate.Value);

            var orders = await query.ToListAsync(cancellationToken);
            var orderIds = orders.Select(o => o.Id).ToList();

            var orderItems = await orderItemRepository.FindAll(x => orderIds.Contains(x.OrderId)).ToListAsync(cancellationToken);

            var report = new List<OrderReportDTO>();

            switch (request.TimeUnit?.ToLower())
            {
                case "day":
                    report = GetDailyReport(orders, orderItems);
                    break;
                case "month":
                    report = GetMonthlyReport(orders, orderItems);
                    break;
                case "year":
                    report = GetYearlyReport(orders, orderItems);
                    break;
                default:
                    return Result<List<OrderReportDTO>>.Errors("Invalid time unit. Must be 'day', 'month', or 'year'.");
            }

            var result = Result<List<OrderReportDTO>>.Ok(report);

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        private List<OrderReportDTO> GetDailyReport(List<Domain.Entities.OrderManagement.Order> orders, List<Domain.Entities.OrderManagement.OrderItem> orderItems)
        {
            return orders
                .GroupBy(o => o.CreatedAt.Value.Date)
                .Select(g =>
                {
                    var orderIds = g.Select(o => o.Id).ToList();
                    var items = orderItems.Where(oi => orderIds.Contains(oi.OrderId)).ToList();

                    var successfulItems = items.Count(oi => oi.ItemStatus == (int)ItemStatus.Delivered);
                    var failedItems = items.Count(oi => oi.ItemStatus == (int)ItemStatus.Cancelled);
                    var totalItems = items.Count;

                    return new OrderReportDTO
                    {
                        Date = g.Key,
                        TotalRevenue = items.Where(oi => oi.ItemStatus == (int)ItemStatus.Delivered).Sum(oi => oi.TotalPrice ?? 0),
                        TotalOrders = g.Count(),
                        SuccessfulOrders = successfulItems,
                        FailedOrders = failedItems,
                        SuccessRate = totalItems > 0 ? (double)successfulItems / totalItems * 100 : 0
                    };
                }).OrderBy(x => x.Date).ToList();
        }

        private List<OrderReportDTO> GetMonthlyReport(List<Domain.Entities.OrderManagement.Order> orders, List<Domain.Entities.OrderManagement.OrderItem> orderItems)
        {
            return orders
                .GroupBy(o => new DateTime(o.CreatedAt.Value.Year, o.CreatedAt.Value.Month, 1))
                .Select(g =>
                {
                    var orderIds = g.Select(o => o.Id).ToList();
                    var items = orderItems.Where(oi => orderIds.Contains(oi.OrderId)).ToList();

                    var successfulItems = items.Count(oi => oi.ItemStatus == (int)ItemStatus.Delivered);
                    var failedItems = items.Count(oi => oi.ItemStatus == (int)ItemStatus.Cancelled);
                    var totalItems = items.Count;

                    return new OrderReportDTO
                    {
                        Date = g.Key,
                        TotalRevenue = items.Where(oi => oi.ItemStatus == (int)ItemStatus.Delivered).Sum(oi => oi.TotalPrice ?? 0),
                        TotalOrders = g.Count(),
                        SuccessfulOrders = successfulItems,
                        FailedOrders = failedItems,
                        SuccessRate = totalItems > 0 ? (double)successfulItems / totalItems * 100 : 0
                    };
                }).OrderBy(x => x.Date).ToList();
        }

        private List<OrderReportDTO> GetYearlyReport(List<Domain.Entities.OrderManagement.Order> orders, List<Domain.Entities.OrderManagement.OrderItem> orderItems)
        {
            return orders
                .GroupBy(o => new DateTime(o.CreatedAt.Value.Year, 1, 1))
                .Select(g =>
                {
                    var orderIds = g.Select(o => o.Id).ToList();
                    var items = orderItems.Where(oi => orderIds.Contains(oi.OrderId)).ToList();

                    var successfulItems = items.Count(oi => oi.ItemStatus == (int)ItemStatus.Delivered);
                    var failedItems = items.Count(oi => oi.ItemStatus == (int)ItemStatus.Cancelled);
                    var totalItems = items.Count;

                    return new OrderReportDTO
                    {
                        Date = g.Key,
                        TotalRevenue = items.Where(oi => oi.ItemStatus == (int)ItemStatus.Delivered).Sum(oi => oi.TotalPrice ?? 0),
                        TotalOrders = g.Count(),
                        SuccessfulOrders = successfulItems,
                        FailedOrders = failedItems,
                        SuccessRate = totalItems > 0 ? (double)successfulItems / totalItems * 100 : 0
                    };
                }).OrderBy(x => x.Date).ToList();
        }
    }
}