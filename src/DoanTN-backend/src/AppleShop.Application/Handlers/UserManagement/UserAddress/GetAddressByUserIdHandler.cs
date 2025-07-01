using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.UserManagement.UserAddress;
using AppleShop.Application.Validators.UserManagement.UserAddress;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.UserAddress
{
    public class GetAddressByUserIdHandler : IRequestHandler<GetAddressByUserIdRequest, Result<List<UserAddressDTO>>>
    {
        private readonly IUserRepository userRepository;
        private readonly IUserAddressRepository userAddressRepository;
        private readonly ICacheService cacheService;

        public GetAddressByUserIdHandler(IUserRepository userRepository, IUserAddressRepository userAddressRepository, ICacheService cacheService)
        {
            this.userRepository = userRepository;
            this.userAddressRepository = userAddressRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<UserAddressDTO>>> Handle(GetAddressByUserIdRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetAddressByUserIdValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var cacheKey = $"user_address_{request.UserId}";
            var cachedResult = await cacheService.GetAsync<Result<List<UserAddressDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var user = await userRepository.FindByIdAsync(request.UserId);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            var userAddresses = userAddressRepository.FindAll(x => x.UserId == request.UserId).ToList();
            var userAddtessDtos = userAddresses.Select(userAddress => new UserAddressDTO
            {
                AddressId = userAddress.Id,
                FullName = $"{userAddress.FirstName} {userAddress.LastName}",
                PhoneNumber = userAddress.PhoneNumber,
                Address = $"{userAddress.AddressLine}, {userAddress.Ward}, {userAddress.District}, {userAddress.Province}"
            }).ToList();

            var result = Result<List<UserAddressDTO>>.Ok(userAddtessDtos);
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
    }
}