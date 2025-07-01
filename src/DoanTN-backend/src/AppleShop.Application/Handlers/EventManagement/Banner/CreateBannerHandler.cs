using AppleShop.Application.Requests.EventManagement.Banner;
using AppleShop.Application.Validators.EventManagement.Banner;
using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities.EventManagement;

namespace AppleShop.Application.Handlers.EventManagement.Banner
{
    public class CreateBannerHandler : IRequestHandler<CreateBannerRequest, Result<object>>
    {
        private readonly IBannerRepository bannerRepository;
        private readonly IFileService fileService;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;

        public CreateBannerHandler(IBannerRepository bannerRepository, IFileService fileService, IMapper mapper, ICacheService cacheService)
        {
            this.bannerRepository = bannerRepository;
            this.fileService = fileService;
            this.mapper = mapper;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateBannerRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateBannerValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var banner = mapper.Map<Entities.Banner>(request);

            using var transaction = await bannerRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                bannerRepository.Create(banner);
                await bannerRepository.SaveChangesAsync(cancellationToken);

                if (request.ImageData is not null)
                {
                    var uploadFile = new UploadFileRequest
                    {
                        Content = request.ImageData,
                        AssetType = AssetType.Banner,
                        Suffix = banner.Id.ToString(),
                    };
                    banner.Url = await fileService.UploadFileAsync(uploadFile);
                }

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