using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Validators.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using System.Security.Claims;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class VerifyResetPasswordHandler : IRequestHandler<VerifyResetPasswordRequest, Result<object>>
    {
        private readonly IUserRepository userRepository;
        private readonly IEmailService emailService;
        private readonly IJwtService jwtService;

        public VerifyResetPasswordHandler(IUserRepository userRepository, IEmailService emailService, IJwtService jwtService)
        {
            this.userRepository = userRepository;
            this.emailService = emailService;
            this.jwtService = jwtService;
        }

        public async Task<Result<object>> Handle(VerifyResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var validator = new VerifyResetPasswordValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var user = await userRepository.FindSingleAsync(x => x.Email == request.Email && x.IsActived == 1, true);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.User));

            using var transaction = await userRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("ResetPassword", "true")
                };

                var resetToken = jwtService.GenerateAccessToken(claims, 15);
                var resetLink = $"localhost:4200/forgot-password?token={resetToken}";

                var subject = "Đặt lại mật khẩu!";
                var body = $@"
                <p>Xin chào,</p>
                <p>Vui lòng nhấn vào link sau để đặt lại mật khẩu:</p>
                <a href='{resetLink}'>Đặt lại mật khẩu</a>
                <p>Link này sẽ hết hạn sau 15 phút.</p>
";
                await emailService.SendEmailAsync(request.Email, subject, body);

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