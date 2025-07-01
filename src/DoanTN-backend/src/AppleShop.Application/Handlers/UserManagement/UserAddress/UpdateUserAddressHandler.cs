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
    public class UpdateUserAddressHandler : IRequestHandler<UpdateUserAddressRequest, Result<object>>
    {
        private readonly IUserAddressRepository userAddressRepository;
        private readonly ICacheService cacheService;

        public UpdateUserAddressHandler(IUserAddressRepository userAddressRepository, ICacheService cacheService)
        {
            this.userAddressRepository = userAddressRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(UpdateUserAddressRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateUserAddressValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var userAddress = await userAddressRepository.FindByIdAsync(request.Id, true);
            if (userAddress is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.UserAddress));

            using var transaction = await userAddressRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                userAddress.FirstName = request.FirstName ?? userAddress.FirstName;
                userAddress.LastName = request.LastName ?? userAddress.LastName;
                userAddress.AddressLine = request.AddressLine ?? userAddress.AddressLine;
                userAddress.PhoneNumber = request.PhoneNumber ?? userAddress.PhoneNumber;
                userAddress.Province = request.Province ?? userAddress.Province;
                userAddress.District = request.District ?? userAddress.District;
                userAddress.Ward = request.Ward ?? userAddress.Ward;
                userAddressRepository.Update(userAddress);
                await userAddressRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.RemoveAsync($"user_address_{userAddress.UserId}");
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