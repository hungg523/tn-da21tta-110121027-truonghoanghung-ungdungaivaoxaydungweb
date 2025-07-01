using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Validators.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordRequest, Result<object>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IUserRepository userRepository;

        public ChangePasswordHandler(IAuthRepository authRepository, IUserRepository userRepository)
        {
            this.authRepository = authRepository;
            this.userRepository = userRepository;
        }

        public async Task<Result<object>> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var validator = new ChangePasswordValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var user = await userRepository.FindByIdAsync(request.UserId, true);
            if (user is null) AppleException.ThrowNotFound();

            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.Password = password;
                userRepository.Update(user);
                await userRepository.SaveChangesAsync();

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