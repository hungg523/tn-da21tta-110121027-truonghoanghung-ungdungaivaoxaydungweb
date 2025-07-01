using AppleShop.Application.Requests.DTOs.StatisticalManagement.SummaryReport;
using AppleShop.Application.Requests.StatisticalManagement.SummaryReport;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppleShop.Application.Handlers.StatisticalManagement.SummaryReport
{
    public class GetSummaryReportHandler : IRequestHandler<GetSummaryReportRequest, Result<List<SummaryReportDTO>>>
    {
        private readonly IProductViewRepository productViewRepository;
        private readonly ICartRepository cartRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IUserRepository userRepository;
        private readonly ICartItemRepository cartItemRepository;
        private readonly ICacheService cacheService;

        public GetSummaryReportHandler(
            IProductViewRepository productViewRepository,
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            ICartItemRepository cartItemRepository,
            ICacheService cacheService)
        {
            this.productViewRepository = productViewRepository;
            this.cartRepository = cartRepository;
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.cartItemRepository = cartItemRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<SummaryReportDTO>>> Handle(GetSummaryReportRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"summary_report_{request.StartDate}_{request.EndDate}_{request.TimeUnit}";
            var cachedResult = await cacheService.GetAsync<Result<List<SummaryReportDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var report = new List<SummaryReportDTO>();

            switch (request.TimeUnit?.ToLower())
            {
                case "day":
                    report = await GetDailyReport(request.StartDate, request.EndDate, cancellationToken);
                    break;
                case "month":
                    report = await GetMonthlyReport(request.StartDate, request.EndDate, cancellationToken);
                    break;
                case "year":
                    report = await GetYearlyReport(request.StartDate, request.EndDate, cancellationToken);
                    break;
                default:
                    return Result<List<SummaryReportDTO>>.Errors("Invalid time unit. Must be 'day', 'month', or 'year'.");
            }

            var result = Result<List<SummaryReportDTO>>.Ok(report);

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        private async Task<List<SummaryReportDTO>> GetDailyReport(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var views = await GetViewsByDate(startDate, endDate, cancellationToken);
            var cartItems = await GetCartItemsByDate(startDate, endDate, cancellationToken);
            var orders = await GetOrdersByDate(startDate, endDate, cancellationToken);
            var customers = await GetCustomersByDate(startDate, endDate, cancellationToken);

            return ProcessReportData(views, cartItems, orders, customers, date => date.Date);
        }

        private async Task<List<SummaryReportDTO>> GetMonthlyReport(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var views = await GetViewsByDate(startDate, endDate, cancellationToken);
            var cartItems = await GetCartItemsByDate(startDate, endDate, cancellationToken);
            var orders = await GetOrdersByDate(startDate, endDate, cancellationToken);
            var customers = await GetCustomersByDate(startDate, endDate, cancellationToken);

            return ProcessReportData(views, cartItems, orders, customers, date => new DateTime(date.Year, date.Month, 1));
        }

        private async Task<List<SummaryReportDTO>> GetYearlyReport(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var views = await GetViewsByDate(startDate, endDate, cancellationToken);
            var cartItems = await GetCartItemsByDate(startDate, endDate, cancellationToken);
            var orders = await GetOrdersByDate(startDate, endDate, cancellationToken);
            var customers = await GetCustomersByDate(startDate, endDate, cancellationToken);

            return ProcessReportData(views, cartItems, orders, customers, date => new DateTime(date.Year, 1, 1));
        }

        private async Task<List<ViewData>> GetViewsByDate(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = productViewRepository.FindAll();

            if (startDate.HasValue) query = query.Where(x => x.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(x => x.CreatedAt <= endDate.Value);

            var results = await query.GroupBy(x => x.CreatedAt.Value).Select(g => new ViewData { Date = g.Key, Count = g.Sum(x => x.View ?? 0) }).ToListAsync(cancellationToken);
            return results;
        }

        private async Task<List<CartData>> GetCartItemsByDate(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = cartRepository.FindAll();

            if (startDate.HasValue) query = query.Where(x => x.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(x => x.CreatedAt <= endDate.Value);

            var results = await query.GroupBy(x => x.CreatedAt.Value.Date).Select(g => new CartData { Date = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);

            return results;
        }

        private async Task<List<OrderData>> GetOrdersByDate(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = orderRepository.FindAll();

            if (startDate.HasValue)query = query.Where(x => x.CreatedAt >= startDate.Value);
            if (endDate.HasValue)query = query.Where(x => x.CreatedAt <= endDate.Value);

            var results = await query.Where(x => x.UserId.HasValue).GroupBy(x => x.CreatedAt.Value.Date)
                .Select(g => new OrderData 
                { 
                    Date = g.Key, 
                    Count = g.Count(),
                    UserIds = g.Select(x => x.UserId.Value).Distinct().ToList()
                }).ToListAsync(cancellationToken);

            return results;
        }

        private async Task<List<CustomerData>> GetCustomersByDate(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = userRepository.FindAll();

            if (startDate.HasValue)query = query.Where(x => x.CreatedAt >= startDate.Value);
            if (endDate.HasValue)query = query.Where(x => x.CreatedAt <= endDate.Value);

            var users = await query.ToListAsync(cancellationToken);
            var allOrders = await orderRepository.FindAll()
                .Where(x => (!startDate.HasValue || x.CreatedAt >= startDate.Value) &&
                           (!endDate.HasValue || x.CreatedAt <= endDate.Value))
                .Select(x => new { x.UserId, x.CreatedAt })
                .ToListAsync(cancellationToken);

            var userFirstOrders = allOrders.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Min(x => x.CreatedAt));

            var results = users.GroupBy(x => x.CreatedAt.Value.Date)
                .Select(g => new CustomerData
                {
                    Date = g.Key,
                    TotalCustomers = g.Count(),
                    ReturningCustomers = g.Count(u =>
                        userFirstOrders.TryGetValue(u.Id, out var firstOrderDate) && firstOrderDate < g.Key)
                }).ToList();

            return results;
        }

        private List<SummaryReportDTO> ProcessReportData(
            List<ViewData> views,
            List<CartData> cartItems,
            List<OrderData> orders,
            List<CustomerData> customers,
            Func<DateTime, DateTime> dateSelector)
        {
            var dates = views.Select(x => dateSelector(x.Date))
                .Union(cartItems.Select(x => dateSelector(x.Date)))
                .Union(orders.Select(x => dateSelector(x.Date)))
                .Union(customers.Select(x => dateSelector(x.Date)))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return dates.Select(date =>
            {
                var dateViews = views.Where(x => dateSelector(x.Date) == date).Sum(x => x.Count);
                var dateCartItems = cartItems.Where(x => dateSelector(x.Date) == date).Sum(x => x.Count);
                var dateOrders = orders.Where(x => dateSelector(x.Date) == date).Sum(x => x.Count);
                var dateCustomers = customers.FirstOrDefault(x => dateSelector(x.Date) == date);

                return new SummaryReportDTO
                {
                    Date = date,
                    TotalViews = dateViews,
                    TotalCartItems = dateCartItems,
                    TotalOrders = dateOrders,
                    ViewToCartRate = dateViews > 0 ? (double)dateCartItems / dateViews * 100 : 0,
                    CartToOrderRate = dateCartItems > 0 ? (double)dateOrders / dateCartItems * 100 : 0,
                    ViewToOrderRate = dateViews > 0 ? (double)dateOrders / dateViews * 100 : 0,
                    TotalCustomers = dateCustomers?.TotalCustomers ?? 0,
                    ReturningCustomers = dateCustomers?.ReturningCustomers ?? 0,
                    ReturningCustomerRate = dateCustomers?.TotalCustomers > 0 ? (double)dateCustomers.ReturningCustomers / dateCustomers.TotalCustomers * 100 : 0
                };
            }).ToList();
        }

        private class ViewData
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
        }

        private class CartData
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
        }

        private class OrderData
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
            public List<int> UserIds { get; set; }
        }

        private class CustomerData
        {
            public DateTime Date { get; set; }
            public int TotalCustomers { get; set; }
            public int ReturningCustomers { get; set; }
        }
    }
} 