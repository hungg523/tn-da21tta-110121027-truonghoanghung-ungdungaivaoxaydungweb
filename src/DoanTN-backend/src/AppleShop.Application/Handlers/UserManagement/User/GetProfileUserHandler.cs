using AppleShop.Application.Requests.DTOs.CategoryManagement.Category;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.UserManagement.User;
using AppleShop.Application.Validators.UserManagement.User;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.User
{
    public class GetProfileUserHandler : IRequestHandler<GetProfileUserRequest, Result<UserDTO>>
    {
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;
        private readonly ICacheService cacheService;

        public GetProfileUserHandler(IUserRepository userRepository, IFileService fileService, ICacheService cacheService)
        {
            this.userRepository = userRepository;
            this.fileService = fileService;
            this.cacheService = cacheService;
        }

        public async Task<Result<UserDTO>> Handle(GetProfileUserRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetProfileUserValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            //var cacheKey = $"profile_{request.Id}";
            //var cachedResult = await cacheService.GetAsync<Result<UserDTO>>(cacheKey);
            //if (cachedResult is not null) return cachedResult;

            var user = await userRepository.FindByIdAsync(request.Id);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            var userDto = new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Avatar = !string.IsNullOrEmpty(user.ImageUrl) && user.ImageUrl.StartsWith("https")
                        ? user.ImageUrl
                        : fileService.GetFullPathFileServer(user.ImageUrl),
            };

            var result = Result<UserDTO>.Ok(userDto);
            //await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
    }
}