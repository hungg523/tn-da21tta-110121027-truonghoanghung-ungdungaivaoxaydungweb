using AppleShop.Application.Requests.DTOs.UserManagement.Auth;
using AppleShop.Application.Requests.UserManagement.Admin;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using System.Security.Claims;
using Entities = AppleShop.Domain.Entities.UserManagement;

namespace AppleShop.Application.Handlers.UserManagement.Admin
{
    public class AdminLoginHandler : IRequestHandler<AdminLoginRequest, Result<LoginDTO>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IJwtService jwtService;
        private readonly IUserRepository userRepository;

        public AdminLoginHandler(IAuthRepository authRepository, IJwtService jwtService, IUserRepository userRepository)
        {
            this.authRepository = authRepository;
            this.jwtService = jwtService;
            this.userRepository = userRepository;
        }

        public async Task<Result<LoginDTO>> Handle(AdminLoginRequest request, CancellationToken cancellationToken)
        {
            var user = await userRepository.FindSingleAsync(x => x.Email == request.Email && x.Role == 1, true);
            if (user is null) AppleException.ThrowNotFound();

            var auth = await authRepository.FindSingleAsync(x => x.UserId == user.Id, true);

            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };
                var accessToken = jwtService.GenerateAccessToken(claims);
                var refreshToken = jwtService.GenerateRefreshToken();

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

                bool isPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                if (!isPassword) AppleException.ThrowException(400, "Password is incorrect.");

                user.LastLogin = DateTime.Now;
                userRepository.Update(user);
                await userRepository.SaveChangesAsync();

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