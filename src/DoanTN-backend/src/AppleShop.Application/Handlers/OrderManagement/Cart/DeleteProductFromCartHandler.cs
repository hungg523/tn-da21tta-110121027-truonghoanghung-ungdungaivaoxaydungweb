using AppleShop.Application.Requests.OrderManagement.Cart;
using AppleShop.Application.Validators.OrderManagement.Cart;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Cart
{
    public class DeleteProductFromCartHandler : IRequestHandler<DeleteProductFromCartRequest, Result<object>>
    {
        private readonly ICartRepository cartRepository;
        private readonly ICartItemRepository cartItemRepository;
        private readonly ICacheService cacheService;

        public DeleteProductFromCartHandler(ICartRepository cartRepository, ICartItemRepository cartItemRepository, ICacheService cacheService)
        {
            this.cartRepository = cartRepository;
            this.cartItemRepository = cartItemRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(DeleteProductFromCartRequest request, CancellationToken cancellationToken)
        {
            var validator = new DeleteProductFromCartValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var cart = await cartRepository.FindSingleAsync(x => x.UserId == request.UserId, true);
            if (cart is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Cart));

            var existingCartItem = await cartItemRepository.FindSingleAsync(x => x.CartId == cart.Id && x.VariantId == request.VariantId, true);
            if (existingCartItem is null) AppleException.ThrowNotFound(message: "Product is not found.");

            using var transaction = await cartRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                cartItemRepository.Delete(existingCartItem);
                await cartItemRepository.SaveChangesAsync(cancellationToken);

                transaction.Commit();

                await cacheService.RemoveAsync($"carts_{request.UserId}");
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