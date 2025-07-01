using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Application.Requests.OrderManagement.Order;
using FluentValidation;

namespace AppleShop.Application.Validators.OrderManagement.Order
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId must not be null.")
            .NotEmpty().WithMessage("UserId must not be empty.")
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");
            RuleFor(x => x.CouponId).GreaterThan(0).WithMessage("CouponId must be greater than 0.");
            RuleFor(x => x.ShipCouponId).GreaterThan(0).WithMessage("ShipCouponId must be greater than 0.");

            RuleForEach(x => x.OrderItems).SetValidator(new OrderItemDTOValidator());
        }
    }

    public class OrderItemDTOValidator : AbstractValidator<OrderItemDTO>
    {
        public OrderItemDTOValidator()
        {
            RuleFor(x => x.VariantId)
                .NotNull().WithMessage("ProductId must not be null.")
                .NotEmpty().WithMessage("ProductId must not be empty.")
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");

            RuleFor(x => x.Quantity)
                .NotNull().WithMessage("Quantity must not be null.")
                .NotEmpty().WithMessage("Quantity must not be empty.")
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}