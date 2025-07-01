using AppleShop.Application.Requests.UserManagement.UserAddress;
using AppleShop.Application.Validators.UserManagement.UserAddress;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.UserAddress
{
    public class GetDetailUserAddressHandler : IRequestHandler<GetDetailUserAddressRequest, Result<Entities.UserManagement.UserAddress>>
    {
        private readonly IUserAddressRepository userAddressRepository;

        public GetDetailUserAddressHandler(IUserAddressRepository userAddressRepository)
        {
            this.userAddressRepository = userAddressRepository;
        }

        public async Task<Result<Entities.UserManagement.UserAddress>> Handle(GetDetailUserAddressRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailUserAddressValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var userAddress = await userAddressRepository.FindByIdAsync(request.Id);
            if (userAddress is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.UserAddress));
            return Result<Entities.UserManagement.UserAddress>.Ok(userAddress);
        }
    }
}