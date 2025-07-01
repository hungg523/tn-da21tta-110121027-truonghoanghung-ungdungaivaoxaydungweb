using AppleShop.Application.Requests.DTOs.UserManagement.Auth;
using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using System.Security.Claims;
using Entities = AppleShop.Domain.Entities.UserManagement;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class GoogleLoginHandler : IRequestHandler<GoogleLoginRequest, Result<LoginDTO>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IUserRepository userRepository;
        private readonly IJwtService jwtService;

        public GoogleLoginHandler(IAuthRepository authRepository, IUserRepository userRepository, IJwtService jwtService)
        {
            this.authRepository = authRepository;
            this.userRepository = userRepository;
            this.jwtService = jwtService;
        }

        public async Task<Result<LoginDTO>> Handle(GoogleLoginRequest request, CancellationToken cancellationToken)
        {
            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = await userRepository.FindSingleAsync(x => x.Email == request.Email, true);
                if (user.IsActived == 2) AppleException.ThrowForbiden("Account has banned.");
                if (user is null)
                {
                    user = new Entities.User
                    {
                        Email = request.Email,
                        Username = request.UserName,
                        ImageUrl = request.ImageUrl,
                        CreatedAt = DateTime.Now,
                        LastLogin = DateTime.Now,
                        Role = 0,
                        IsActived = 1
                    };

                    userRepository.Create(user);
                    await userRepository.SaveChangesAsync();
                }
                else
                {
                    user.IsActived = 1;
                    user.LastLogin = DateTime.Now;
                    userRepository.Update(user);
                    await userRepository.SaveChangesAsync();
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };
                var accessToken = jwtService.GenerateAccessToken(claims);
                var refreshToken = jwtService.GenerateRefreshToken();

                var auth = await authRepository.FindSingleAsync(x => x.UserId == user.Id, true);
                if (auth is null)
                {
                    var userToken = new Entities.Auth
                    {
                        UserId = user.Id,
                        RefreshToken = refreshToken,
                        IssuedAt = DateTime.Now,
                        Expiration = DateTime.Now.AddDays(7),
                        IsActived = 1
                    };
                    authRepository.Create(userToken);
                }
                else
                {
                    auth.RefreshToken = refreshToken;
                    auth.IssuedAt = DateTime.Now;
                    auth.Expiration = DateTime.Now.AddDays(7);
                    auth.IsActived = 1;
                    authRepository.Update(auth);
                }

                await authRepository.SaveChangesAsync(cancellationToken);

                var response = new LoginDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                transaction.Commit();
                return Result<LoginDTO>.Ok(response);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}