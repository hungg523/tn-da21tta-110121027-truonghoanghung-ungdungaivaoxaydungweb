using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Validators.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class LogoutHandler : IRequestHandler<LogoutRequest, Result<object>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;

        public LogoutHandler(IAuthRepository authRepository, IMapper mapper, IUserRepository userRepository)
        {
            this.authRepository = authRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        public async Task<Result<object>> Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            var validator = new LogoutValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var user = await userRepository.FindByIdAsync(request.UserId, false);
            if (user is null) AppleException.ThrowNotFound();

            var userToken = await authRepository.FindSingleAsync(x => x.UserId == user.Id, true);
            if (userToken is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.Auth));

            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                userToken.IsActived = 0;
                userToken.RevokedAt = DateTime.Now;
                authRepository.Update(userToken);
                await authRepository.SaveChangesAsync(cancellationToken);
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