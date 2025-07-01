using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Validators.UserManagement.Auth;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.UserManagement.Auth
{
    public class VerifyOTPHandler : IRequestHandler<VerifyOTPRequest, Result<object>>
    {
        private readonly IAuthRepository authRepository;
        private readonly IUserRepository userRepository;

        public VerifyOTPHandler(IAuthRepository authRepository, IUserRepository userRepository)
        {
            this.authRepository = authRepository;
            this.userRepository = userRepository;
        }

        public async Task<Result<object>> Handle(VerifyOTPRequest request, CancellationToken cancellationToken)
        {
            var validator = new VerifyOTPValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var userExist = await userRepository.FindSingleAsync(x => x.Email == request.Email && x.IsActived == 1, true);
            if (userExist is not null) AppleException.ThrowConflict("Email has activated");

            using var transaction = await authRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = await userRepository.FindSingleAsync(x => x.Email == request.Email && x.OTP == request.OTP, true);
                if (user is null)
                {
                    AppleException.ThrowConflict("OTP is incorrect.");
                    user.OTPAttempt += 1;
                    userRepository.Update(user);
                    await userRepository.SaveChangesAsync();
                }

                if (user.OTPExpiration < DateTime.Now) AppleException.ThrowConflict("OTP is expired.");

                if (user.OTPAttempt > 5) AppleException.ThrowConflict("OTP has been entered more than 5 times.");

                user.IsActived = 1;
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