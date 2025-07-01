using AppleShop.Application.Requests.WishListManagement.WishList;
using AppleShop.Application.Validators.WishListManagement.WishList;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Abstractions.IRepositories.WishListManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities.WishListManagement;

namespace AppleShop.Application.Handlers.WishListManagement.WishList
{
    public class CreateWishListHandler : IRequestHandler<CreateWishListRequest, Result<object>>
    {
        private readonly IWishListRepository wishListRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly ICacheService cacheService;

        public CreateWishListHandler(IWishListRepository wishListRepository, IMapper mapper, IUserRepository userRepository, ICacheService cacheService)
        {
            this.wishListRepository = wishListRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(CreateWishListRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateWishListValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);
            
            var user = await userRepository.FindSingleAsync(x => x.Id == request.UserId && x.IsActived == 1);
            if (user is null) AppleException.ThrowUnAuthorization("Please log in.");
            
            using var transaction = await wishListRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var existedWishList = await wishListRepository.FindSingleAsync(x => x.VariantId == request.VariantId && x.UserId == user.Id, true);
                if (existedWishList is not null)
                {
                    existedWishList.IsActived = true;
                    wishListRepository.Update(existedWishList);
                }
                
                if (existedWishList is null)
                {
                    var wishList = mapper.Map<Entities.WishList>(request);
                    wishListRepository.Create(wishList);
                }
                
                await wishListRepository.SaveChangesAsync(cancellationToken);

                transaction.Commit();

                await cacheService.RemoveAsync($"wish_list_{request.UserId}");
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