using AppleShop.Application.Requests.UserManagement.UserAddress;
using AppleShop.Application.Validators.UserManagement.UserAddress;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.UserAddress
{
    public class CreateUserAddressHandler : IRequestHandler<CreateUserAddressRequest, Result<object>>
    {
        private readonly IUserAddressRepository userAddressRepository;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;

        public CreateUserAddressHandler(IUserAddressRepository userAddressRepository, IMapper mapper, ICacheService cacheService)
        {
            this.userAddressRepository = userAddressRepository;
            this.mapper = mapper;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateUserAddressRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateUserAddressValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var userAddress = mapper.Map<Entities.UserManagement.UserAddress>(request);

            using var transaction = await userAddressRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                userAddressRepository.Create(userAddress);
                await userAddressRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();

                await cacheService.RemoveAsync($"user_address_{request.UserId}");
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