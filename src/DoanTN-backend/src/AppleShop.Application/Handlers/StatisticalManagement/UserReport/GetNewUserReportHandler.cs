using AppleShop.Application.Requests.DTOs.StatisticalManagement.UserReport;
using AppleShop.Application.Requests.StatisticalManagement.UserReport;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.StatisticalManagement.UserReport
{
    public class GetNewUserReportHandler : IRequestHandler<GetNewUserReportRequest, Result<List<NewUserReportDTO>>>
    {
        private readonly IUserRepository userRepository;
        private readonly ICacheService cacheService;

        public GetNewUserReportHandler(IUserRepository userRepository, ICacheService cacheService)
        {
            this.userRepository = userRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<NewUserReportDTO>>> Handle(GetNewUserReportRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"new_user_report_{request.StartDate}_{request.EndDate}";
            var cachedResult = await cacheService.GetAsync<Result<List<NewUserReportDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var startDate = request.StartDate ?? DateTime.Now.AddDays(-30);
            var endDate = request.EndDate ?? DateTime.Now;

            var users = userRepository.FindAll(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate).ToList();

            var report = users.GroupBy(x => x.CreatedAt.Value)
                .Select(g => new NewUserReportDTO
                {
                    Date = g.Key,
                    NewUserCount = g.Count()
                }).OrderBy(x => x.Date).ToList();

            var result = Result<List<NewUserReportDTO>>.Ok(report);

            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
    }
}