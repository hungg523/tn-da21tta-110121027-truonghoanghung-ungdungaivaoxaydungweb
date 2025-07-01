using AppleShop.Application.Requests.EventManagement.Banner;
using AppleShop.Application.Validators.EventManagement.Banner;
using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities.EventManagement;

namespace AppleShop.Application.Handlers.EventManagement.Banner
{
    public class DeleteBannerHandler : IRequestHandler<DeleteBannerRequest, Result<object>>
    {
        private readonly IBannerRepository bannerRepository;
        private readonly ICacheService cacheService;

        public DeleteBannerHandler(IBannerRepository bannerRepository, ICacheService cacheService)
        {
            this.bannerRepository = bannerRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(DeleteBannerRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteBannerValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var banner = await bannerRepository.FindByIdAsync(request.Id, true);
            if (banner is null) AppleException.ThrowNotFound(typeof(Entities.Banner));

            using var transaction = await bannerRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                banner.Status = 0;
                bannerRepository.Update(banner);
                await bannerRepository.SaveChangesAsync(cancellationToken);
                await cacheService.RemoveByPatternAsync("banners_*");
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