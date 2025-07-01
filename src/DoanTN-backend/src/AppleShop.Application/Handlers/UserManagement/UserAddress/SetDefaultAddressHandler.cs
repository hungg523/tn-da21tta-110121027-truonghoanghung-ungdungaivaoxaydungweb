using AppleShop.Application.Requests.UserManagement.UserAddress;
using AppleShop.Application.Validators.UserManagement.UserAddress;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.UserAddress
{
    public class SetDefaultAddressHandler : IRequestHandler<SetDefaultAddressRequest, Result<object>>
    {
        private readonly IUserAddressRepository userAddressRepository;

        public SetDefaultAddressHandler(IUserAddressRepository userAddressRepository)
        {
            this.userAddressRepository = userAddressRepository;
        }

        public async Task<Result<object>> Handle(SetDefaultAddressRequest request, CancellationToken cancellationToken)
        {
            var validator = new SetDefaultAddressValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var userAddress = await userAddressRepository.FindByIdAsync(request.Id, true);
            if (userAddress is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.UserAddress));

            using var transaction = await userAddressRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var userAddresses = userAddressRepository.FindAll(x => x.UserId == userAddress.UserId, true).ToList();

                foreach (var addr in userAddresses)
                {
                    addr.Default = false;
                    userAddressRepository.Update(addr);
                }

                userAddress.Default = true;
                userAddressRepository.Update(userAddress);
                await userAddressRepository.SaveChangesAsync(cancellationToken);
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