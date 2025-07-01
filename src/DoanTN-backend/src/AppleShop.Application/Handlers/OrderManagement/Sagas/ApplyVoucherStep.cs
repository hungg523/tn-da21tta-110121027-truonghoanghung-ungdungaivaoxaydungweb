using AppleShop.Application.Requests.DTOs.OrderManagement.Saga;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Sagas
{
    public class ApplyVoucherStep : ISagaStep
    {
        private readonly ICouponRepository couponRepository;
        private readonly ICouponTypeRepository couponTypeRepository;
        private readonly IUserCouponRepository userCouponRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly CreateOrderRequest request;
        private readonly OrderSagaDTO orderSagaDTO;

        private Entities.PromotionManagement.Coupon? usedCoupon;
        private Entities.PromotionManagement.Coupon? usedShipCoupon;
        private Entities.UserManagement.UserCoupon? userCoupon;
        private Entities.UserManagement.UserCoupon? userShipCoupon;

        public ApplyVoucherStep(ICouponRepository couponRepository, ICouponTypeRepository couponTypeRepository, IUserCouponRepository userCouponRepository, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, CreateOrderRequest request, OrderSagaDTO orderSagaDTO)
        {
            this.couponRepository = couponRepository;
            this.couponTypeRepository = couponTypeRepository;
            this.userCouponRepository = userCouponRepository;
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.request = request;
            this.orderSagaDTO = orderSagaDTO;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var order = await orderRepository.FindSingleAsync(x => x.OrderCode == orderSagaDTO.Code, true);
            var orderItems = orderItemRepository.FindAll(x => x.OrderId == order.Id, true).ToList();

            if (request.CouponId is not null)
            {
                var totalAmount = orderItems.Sum(x => x.TotalPrice);
                Entities.PromotionManagement.Coupon? coupon = new();

                if (request.CouponId is not null) coupon = await couponRepository.FindSingleAsync(x => x.Id == request.CouponId && x.TimesUsed < x.MaxUsage && x.MinOrderValue <= totalAmount && x.EndDate > DateTime.Now && x.IsActived == true, true);
                else coupon = await couponRepository.FindSingleAsync(x => x.Code == request.Code && x.TimesUsed < x.MaxUsage && x.MinOrderValue <= totalAmount && x.EndDate > DateTime.Now && x.IsActived == true, true);

                if (coupon is null) AppleException.ThrowNotFound(message: "Voucher is invalid.");

                var couponVoucher = await couponTypeRepository.FindSingleAsync(x => x.Id == coupon.CtId && x.Name == (int)CouponType.ORDER);
                if (couponVoucher is null) AppleException.ThrowNotFound(message: "Voucher is invalid.");

                await ValidateUserCoupon(request.UserId.Value, coupon, userCouponRepository);

                decimal totalDiscount = coupon.DiscountPercentage > 0
                        ? Math.Min((decimal)totalAmount * (decimal)coupon.DiscountPercentage / (decimal)100.00, coupon.MaxDiscountAmount ?? decimal.MaxValue)
                        : coupon.DiscountAmount.Value;

                var discountPerItem = totalDiscount / totalAmount;

                foreach (var item in orderItems)
                {
                    item.FinalPrice = item.OriginalPrice * (1 - discountPerItem);
                    if (item.FinalPrice < 0) item.FinalPrice = 0;
                    item.TotalPrice = item.FinalPrice * item.Quantity;
                    orderItemRepository.Update(item);
                }

                await orderItemRepository.SaveChangesAsync(cancellationToken);

                var couponType = await couponTypeRepository.FindSingleAsync(x => x.Id == coupon.CtId, true);
                if (couponType is null) AppleException.ThrowNotFound(typeof(Entities.PromotionManagement.CouponType));

                coupon.TimesUsed += 1;
                couponRepository.Update(coupon);
                await couponRepository.SaveChangesAsync(cancellationToken);

                order.CouponId = coupon.Id;
                orderRepository.Update(order);

                usedCoupon = coupon;
            }

            if (request.ShipCouponId is not null)
            {
                var totalAmount = orderItems.Sum(x => x.TotalPrice);

                var shipCoupon = await couponRepository.FindSingleAsync(x => x.Id == request.ShipCouponId && x.TimesUsed < x.MaxUsage && x.MinOrderValue <= totalAmount && x.EndDate > DateTime.Now && x.IsActived == true, true);
                if (shipCoupon is null) AppleException.ThrowNotFound(message: "Shipping voucher is invalid.");

                var shipVoucher = await couponTypeRepository.FindSingleAsync(x => x.Id == shipCoupon.Id && x.Name == (int)CouponType.SHIP);
                if (shipVoucher is null) AppleException.ThrowNotFound(message: "Shipping voucher is invalid.");

                await ValidateUserCoupon(request.UserId.Value, shipCoupon, userCouponRepository);

                shipCoupon.TimesUsed += 1;
                couponRepository.Update(shipCoupon);
                await couponRepository.SaveChangesAsync(cancellationToken);

                order.ShipFee -= shipCoupon.DiscountAmount;
                if (order.ShipFee < 0) order.ShipFee = 0;

                order.ShipCouponId = shipCoupon.Id;
                orderRepository.Update(order);

                usedShipCoupon = shipCoupon;
            }

            if (orderItems.Sum(x => x.TotalPrice) >= 2000000) order.ShipFee = 0;
            order.TotalAmount = orderItems.Sum(x => x.TotalPrice) + order.ShipFee;
            orderRepository.Update(order);
            await orderRepository.SaveChangesAsync(cancellationToken);

            orderSagaDTO.Amount = order.TotalAmount;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (usedCoupon is not null)
            {
                usedCoupon.TimesUsed -= 1;
                couponRepository.Update(usedCoupon);
            }

            if (usedShipCoupon is not null)
            {
                usedShipCoupon.TimesUsed -= 1;
                couponRepository.Update(usedShipCoupon);
            }

            userCoupon = await userCouponRepository.FindSingleAsync(x => x.CouponId == usedCoupon.Id && usedCoupon.UserSpecific == true, true);
            if (userCoupon is not null)
            {
                userCoupon.IsUsed = false;
                userCoupon.TimesUsed -= 1;
                userCouponRepository.Update(userCoupon);
            }

            userShipCoupon = await userCouponRepository.FindSingleAsync(x => x.CouponId == usedShipCoupon.Id && usedCoupon.UserSpecific == true, true);
            if (userShipCoupon is not null)
            {
                userShipCoupon.IsUsed = false;
                userShipCoupon.TimesUsed -= 1;
                userCouponRepository.Update(userShipCoupon);
            }

            await couponRepository.SaveChangesAsync(cancellationToken);
            await userCouponRepository.SaveChangesAsync(cancellationToken);
        }

        private async Task ValidateUserCoupon(int userId, Entities.PromotionManagement.Coupon coupon, IUserCouponRepository userCouponRepository)
        {
            if (coupon.UserSpecific == true)
            {
                var userCoupon = await userCouponRepository.FindSingleAsync(x => x.UserId == userId && x.CouponId == coupon.Id, true);
                if (userCoupon is null) AppleException.ThrowNotFound(message: "You must have this voucher in advance.");

                userCoupon.IsUsed = true;
                userCoupon.TimesUsed += 1;
                userCouponRepository.Update(userCoupon);
                await userCouponRepository.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}