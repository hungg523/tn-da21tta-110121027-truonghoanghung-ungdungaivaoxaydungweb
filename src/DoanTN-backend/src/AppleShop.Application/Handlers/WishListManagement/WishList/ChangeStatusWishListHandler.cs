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
    public class ChangeStatusWishListHandler : IRequestHandler<ChangeStatusWishListRequest, Result<object>>
    {
        private readonly IWishListRepository wishListRepository;
        private readonly IUserRepository userRepository;
        private readonly ICacheService cacheService;

        public ChangeStatusWishListHandler(IWishListRepository wishListRepository, IUserRepository userRepository, ICacheService cacheService)
        {
            this.wishListRepository = wishListRepository;
            this.userRepository = userRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(ChangeStatusWishListRequest request, CancellationToken cancellationToken)
        {
            var validator = new ChangeStatusWishListValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var wishList = await wishListRepository.FindSingleAsync(x=> x.VariantId == request.VariantId, true);
            if (wishList is null) AppleException.ThrowNotFound(typeof(Entities.WishList));

            var user = await userRepository.FindSingleAsync(x => x.Id == request.UserId && x.IsActived == 1);
            if (user is null) AppleException.ThrowUnAuthorization("Please log in.");

            using var transaction = await wishListRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                wishList!.IsActived = request.IsActived ?? wishList.IsActived;
                wishListRepository.Update(wishList);
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