using AppleShop.Application.Requests.DTOs.UserManagement.Auth;
using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using System.Security.Claims;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, Result<LoginDTO>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IJwtService jwtService;
        private readonly IUserRepository userRepository;

        public RefreshTokenHandler(IAuthRepository authRepository, IJwtService jwtService, IUserRepository userRepository)
        {
            this.authRepository = authRepository;
            this.jwtService = jwtService;
            this.userRepository = userRepository;
        }

        public async Task<Result<LoginDTO>> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var authToken = await authRepository.FindSingleAsync(x => x.RefreshToken == request.RefreshToken && x.IsActived == 1, true);
            if (authToken is null || authToken.Expiration < DateTime.Now) return Result<object>.Failure();

            var user = await userRepository.FindByIdAsync(authToken.UserId);
            if (user is null) AppleException.ThrowNotFound();
            if (user.IsActived == 2) AppleException.ThrowForbiden("Account has banned.");

            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };
                var newAccessToken = jwtService.GenerateAccessToken(claims);
                var newRefreshToken = jwtService.GenerateRefreshToken();
                authToken.RefreshToken = newRefreshToken;
                authToken.IssuedAt = DateTime.Now;
                authToken.Expiration = DateTime.Now.AddDays(7);
                authToken.IsActived = 1;
                authRepository.Update(authToken);
                await authRepository.SaveChangesAsync(cancellationToken);

                var response = new LoginDTO
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };

                transaction.Commit();
                return Result<LoginDTO>.Ok(response);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}