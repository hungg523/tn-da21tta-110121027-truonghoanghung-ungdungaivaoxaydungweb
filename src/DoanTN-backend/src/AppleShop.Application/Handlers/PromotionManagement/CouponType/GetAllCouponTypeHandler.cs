using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Application.Requests.PromotionManagement.CouponType;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleEnum = AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.PromotionManagement.CouponType
{
    public class GetAllCouponTypeHandler : IRequestHandler<GetAllCouponTypeRequest, Result<List<CouponTypeDTO>>>
    {
        private readonly ICouponTypeRepository couponTypeRepository;

        public GetAllCouponTypeHandler(ICouponTypeRepository couponTypeRepository)
        {
            this.couponTypeRepository = couponTypeRepository;
        }

        public async Task<Result<List<CouponTypeDTO>>> Handle(GetAllCouponTypeRequest request, CancellationToken cancellationToken)
        {
            var couponTypes = couponTypeRepository.FindAll().ToList();
            var couponTypeDtos = couponTypes.Select(ct => new CouponTypeDTO
            {
                TypeId = ct.Id.Value,
                TypeName = ((AppleEnum.CouponType)ct.Name).ToString(),
                Description = ct.Description
            }).ToList();

            return Result<List<CouponTypeDTO>>.Ok(couponTypeDtos);
        }
    }
}