using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Validators.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class ResendOTPHandler : IRequestHandler<ResendOTPRequest, Result<object>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IUserRepository userRepository;
        private readonly IEmailService emailService;

        public ResendOTPHandler(IAuthRepository authRepository, IUserRepository userRepository, IEmailService emailService)
        {
            this.authRepository = authRepository;
            this.userRepository = userRepository;
            this.emailService = emailService;
        }

        public async Task<Result<object>> Handle(ResendOTPRequest request, CancellationToken cancellationToken)
        {
            var validator = new ResendOTPValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var user = await userRepository.FindSingleAsync(x => x.Email == request.Email, true);
            if (user is not null && user.IsActived == 1) AppleException.ThrowConflict("Email has activated.");
            if (user.IsActived == 2) AppleException.ThrowForbiden("Account has banned.");
            if (user is null) AppleException.ThrowNotFound();

            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var otp = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                var otpexpiration = DateTime.Now.AddSeconds(90);

                user.OTP = otp;
                user.OTPAttempt = 0;
                user.OTPExpiration = otpexpiration;

                userRepository.Update(user);
                await userRepository.SaveChangesAsync(cancellationToken);
                var subject = "Xác thực tài khoản!";
                var body = $"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>\r\n    <div style='padding: 20px; border-bottom: 3px solid #007BFF; text-align: center;'>\r\n        <img src='https://drive.google.com/uc?export=view&id=1TLWTKoXTzjte2wyn0jnHjTrGUGdcbv98' alt='Logo'\r\n            style='width: 150px; margin-bottom: 10px;' />\r\n        <h2 style='color: #007BFF; font-weight: bold; margin: 0;'>Xác thực tài khoản</h2>\r\n    </div>\r\n    <div style='padding: 20px;'>\r\n        <p>Xin chào,<br> Cảm ơn bạn đã đăng ký tài khoản tại hệ thống của chúng tôi. Để hoàn tất quá trình đăng ký, vui lòng sử dụng mã xác thực bên dưới:</p>\r\n        <div\r\n            style='margin: 20px 0; padding: 15px; background-color: #e9ecef; border-radius: 8px; text-align: center; font-size: 24px; font-weight: bold; color: #007BFF;'>\r\n            {otp}\r\n        </div>\r\n        <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>\r\n        <p>Trân trọng,<br />Hưng AppleShop!</p>\r\n    </div>\r\n    <div\r\n        style='background-color: #343a40; color: white; padding: 10px; text-align: center; font-size: 12px; border-top: 3px solid #007BFF;'>\r\n        © 2024 Hưng AppleShop\r\n    </div>\r\n</div>";
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