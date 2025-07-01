using AppleShop.Application.Requests.DTOs.EventManagement.Banner;
using AppleShop.Application.Requests.EventManagement.Banner;
using AppleShop.Application.Validators.EventManagement.Banner;
using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.EventManagement.Banner
{
    public class GetAllBannerHandler : IRequestHandler<GetAllBannerRequest, Result<List<BannerDTO>>>
    {
        private readonly IBannerRepository bannerRepository;
        private readonly IFileService fileService;
        private readonly IUserRepository userRepository;
        private readonly ICacheService cacheService;

        public GetAllBannerHandler(IBannerRepository bannerRepository, IFileService fileService, IUserRepository userRepository, ICacheService cacheService)
        {
            this.bannerRepository = bannerRepository;
            this.fileService = fileService;
            this.userRepository = userRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<BannerDTO>>> Handle(GetAllBannerRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetAllBannerValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var cacheKey = $"banners_{request.Status}_{request.Position}";
            var cachedResult = await cacheService.GetAsync<Result<List<BannerDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var user = await userRepository.FindSingleAsync(x => x.Id == request.UserId && x.Role != 0, false);
            var banners = bannerRepository.FindAll();
            if (user is null) banners = banners.Where(x => x.StartDate <= DateTime.Now && x.EndDate > DateTime.Now);

            var bannerDtos = banners.Select(banner => new BannerDTO
            {
                Id = banner.Id,
                Title = banner.Title,
                Description = banner.Description,
                Url = fileService.GetFullPathFileServer(banner.Url),
                Link = banner.Link,
                StartDate = banner.StartDate,
                EndDate = banner.EndDate,
                Status = banner.Status,
                Position = banner.Position
            });

            if (request.Position is not null) bannerDtos = bannerDtos.Where(x => x.Position == request.Position);
            if (request.Status is not null) bannerDtos = bannerDtos.Where(x => x.Status == request.Status);

            var result = Result<List<BannerDTO>>.Ok(bannerDtos.ToList());
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
    }
}