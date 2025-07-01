using AppleShop.Application.Requests.UserManagement.User;
using AppleShop.Application.Validators.UserManagement.User;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.UserManagement.User
{
    public class UpdateProfileUserHandler : IRequestHandler<UpdateProfileUserRequest, Result<object>>
    {
        private readonly IUserRepository userRepository;
        private readonly IFileService fileUploadService;
        private readonly IMapper mapper;

        public UpdateProfileUserHandler(IUserRepository userRepository, IFileService fileUploadService, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.fileUploadService = fileUploadService;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(UpdateProfileUserRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateProfileUserValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var user = await userRepository.FindByIdAsync(request.Id, true);
            if (user is null) AppleException.ThrowNotFound(typeof(Entities.UserManagement.UserAddress));

            using var transaction = await userRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                mapper.Map(request, user);
                userRepository.Update(user);
                if (request.ImageData is not null)
                {
                    var uploadFile = new UploadFileRequest
                    {
                        Content = request.ImageData,
                        AssetType = AssetType.User,
                        Suffix = user.Id.ToString()
                    };
                    user.ImageUrl = await fileUploadService.UploadFileAsync(uploadFile);
                }

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