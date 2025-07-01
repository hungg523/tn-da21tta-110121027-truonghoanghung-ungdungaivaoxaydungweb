using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Validators.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Constant;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, Result<object>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IUserRepository userRepository;
        private readonly IConfiguration configuration;

        public ResetPasswordHandler(IAuthRepository authRepository, IUserRepository userRepository, IConfiguration configuration)
        {
            this.authRepository = authRepository;
            this.userRepository = userRepository;
            this.configuration = configuration;
        }

        public async Task<Result<object>> Handle(ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var validator = new ResetPasswordValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection(Const.AUTHEN_KEY).Value!)),
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(request.Token, tokenValidationParameters, out SecurityToken securityToken);
            if (principal is null) AppleException.ThrowUnAuthorization("Invalid or expired reset token.");

            if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                AppleException.ThrowException(400, "Invalid Token");

            var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) AppleException.ThrowUnAuthorization("Invalid token payload.");

            var user = await userRepository.FindSingleAsync(x => x.Email == userEmail && x.IsActived == 1, true);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
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