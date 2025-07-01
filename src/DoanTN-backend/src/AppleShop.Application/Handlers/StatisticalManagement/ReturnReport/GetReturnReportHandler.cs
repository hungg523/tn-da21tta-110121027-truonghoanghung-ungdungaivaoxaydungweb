using AppleShop.Application.Requests.DTOs.StatisticalManagement.ReturnReport;
using AppleShop.Application.Requests.StatisticalManagement.ReturnReport;
using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppleShop.Application.Handlers.StatisticalManagement.ReturnReport
{
    public class GetReturnReportHandler : IRequestHandler<GetReturnReportRequest, Result<List<ReturnReportDTO>>>
    {
        private readonly IReturnRepository returnRepository;
        private readonly ICacheService cacheService;

        public GetReturnReportHandler(IReturnRepository returnRepository, ICacheService cacheService)
        {
            this.returnRepository = returnRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<ReturnReportDTO>>> Handle(GetReturnReportRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"return_report_{request.StartDate}_{request.EndDate}_{request.TimeUnit}_{request.TopReasonsCount}";
            var cachedResult = await cacheService.GetAsync<Result<List<ReturnReportDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var query = returnRepository.FindAll();

            if (request.StartDate.HasValue) query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.CreatedAt <= request.EndDate.Value);

            var returns = await query.ToListAsync(cancellationToken);

            var report = new List<ReturnReportDTO>();
            var topReasonsCount = request.TopReasonsCount ?? 5;

            switch (request.TimeUnit?.ToLower())
            {
                case "day":
                    report = GetDailyReport(returns, topReasonsCount);
                    break;
                case "month":
                    report = GetMonthlyReport(returns, topReasonsCount);
                    break;
                case "year":
                    report = GetYearlyReport(returns, topReasonsCount);
                    break;
                default:
                    return Result<List<ReturnReportDTO>>.Errors("Invalid time unit. Must be 'day', 'month', or 'year'.");
            }

            var result = Result<List<ReturnReportDTO>>.Ok(report);

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        private List<ReturnReportDTO> GetDailyReport(List<Domain.Entities.ReturnManagement.Return> returns, int topReasonsCount)
        {
            return returns
                .GroupBy(r => r.CreatedAt.Value.Date)
                .Select(g =>
                {
                    var totalReturns = g.Count();
                    var totalRefundAmount = g.Sum(r => r.RefundAmount ?? 0);

                    var reasons = g.GroupBy(r => r.Reason)
                        .Select(rg => new ReturnReasonDTO
                        {
                            Reason = rg.Key ?? "Không có lý do",
                            Count = rg.Count(),
                            Percentage = totalReturns > 0 ? (double)rg.Count() / totalReturns * 100 : 0
                        }).OrderByDescending(r => r.Count).Take(topReasonsCount).ToList();

                    return new ReturnReportDTO
                    {
                        Date = g.Key,
                        TotalReturns = totalReturns,
                        TotalRefundAmount = totalRefundAmount,
                        TopReasons = reasons
                    };
                }).OrderBy(x => x.Date).ToList();
        }

        private List<ReturnReportDTO> GetMonthlyReport(List<Domain.Entities.ReturnManagement.Return> returns, int topReasonsCount)
        {
            return returns.GroupBy(r => new DateTime(r.CreatedAt.Value.Year, r.CreatedAt.Value.Month, 1))
                .Select(g =>
                {
                    var totalReturns = g.Count();
                    var totalRefundAmount = g.Sum(r => r.RefundAmount ?? 0);

                    var reasons = g
                        .GroupBy(r => r.Reason)
                        .Select(rg => new ReturnReasonDTO
                        {
                            Reason = rg.Key ?? "Không có lý do",
                            Count = rg.Count(),
                            Percentage = totalReturns > 0 ? (double)rg.Count() / totalReturns * 100 : 0
                        }).OrderByDescending(r => r.Count).Take(topReasonsCount).ToList();

                    return new ReturnReportDTO
                    {
                        Date = g.Key,
                        TotalReturns = totalReturns,
                        TotalRefundAmount = totalRefundAmount,
                        TopReasons = reasons
                    };
                }).OrderBy(x => x.Date).ToList();
        }

        private List<ReturnReportDTO> GetYearlyReport(List<Domain.Entities.ReturnManagement.Return> returns, int topReasonsCount)
        {
            return returns
                .GroupBy(r => new DateTime(r.CreatedAt.Value.Year, 1, 1))
                .Select(g =>
                {
                    var totalReturns = g.Count();
                    var totalRefundAmount = g.Sum(r => r.RefundAmount ?? 0);

                    var reasons = g
                        .GroupBy(r => r.Reason)
                        .Select(rg => new ReturnReasonDTO
                        {
                            Reason = rg.Key ?? "Không có lý do",
                            Count = rg.Count(),
                            Percentage = totalReturns > 0 ? (double)rg.Count() / totalReturns * 100 : 0
                        }).OrderByDescending(r => r.Count).Take(topReasonsCount).ToList();

                    return new ReturnReportDTO
                    {
                        Date = g.Key,
                        TotalReturns = totalReturns,
                        TotalRefundAmount = totalRefundAmount,
                        TopReasons = reasons
                    };
                }).OrderBy(x => x.Date).ToList();
        }
    }
}