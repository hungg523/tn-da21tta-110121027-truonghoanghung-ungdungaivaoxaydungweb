using AppleShop.Application.Requests.UserManagement.Admin;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using Entities = AppleShop.Domain.Entities.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.UserManagement.Admin
{
    public class ChangeStatusUserHandler : IRequestHandler<ChangeStatusUserRequest, Result<object>>
    {
        private readonly IUserRepository userRepository;

        public ChangeStatusUserHandler(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<Result<object>> Handle(ChangeStatusUserRequest request, CancellationToken cancellationToken)
        {
            var user = await userRepository.FindByIdAsync(request.UserId, true);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.User));

            using var transaction = await userRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                user.IsActived = request.IsActived ?? user.IsActived;
                userRepository.Update(user);
                await userRepository.SaveChangesAsync(cancellationToken);
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