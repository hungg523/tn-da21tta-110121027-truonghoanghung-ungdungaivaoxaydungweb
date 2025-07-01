using AppleShop.Application.Requests.CategoryManagement.Category;
using AppleShop.Application.Requests.DTOs.OrderManagement.Cart;
using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Application.Requests.DTOs.PromotionManagement.Coupon;
using AppleShop.Application.Requests.EventManagement.Banner;
using AppleShop.Application.Requests.OrderManagement.Cart;
using AppleShop.Application.Requests.ProductManagement.Attribute;
using AppleShop.Application.Requests.ProductManagement.AttributeValue;
using AppleShop.Application.Requests.ProductManagement.Product;
using AppleShop.Application.Requests.ProductManagement.ProductDetail;
using AppleShop.Application.Requests.ProductManagement.ProductImage;
using AppleShop.Application.Requests.ProductManagement.ProductVariant;
using AppleShop.Application.Requests.PromotionManagement.Coupon;
using AppleShop.Application.Requests.PromotionManagement.CouponType;
using AppleShop.Application.Requests.PromotionManagement.ProductPromotion;
using AppleShop.Application.Requests.PromotionManagement.Promotion;
using AppleShop.Application.Requests.ReturnManagement.Return;
using AppleShop.Application.Requests.ReviewManagement.Review;
using AppleShop.Application.Requests.ReviewManagement.ReviewReport;
using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Requests.UserManagement.User;
using AppleShop.Application.Requests.UserManagement.UserAddress;
using AppleShop.Application.Requests.WishListManagement.WishList;
using AppleShop.Domain.Entities.CategoryManagement;
using AppleShop.Domain.Entities.EventManagement;
using AppleShop.Domain.Entities.OrderManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Domain.Entities.PromotionManagement;
using AppleShop.Domain.Entities.ReturnManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using AppleShop.Domain.Entities.UserManagement;
using AppleShop.Domain.Entities.WishListManagement;
using AutoMapper;

namespace AppleShop.Application.Mapping
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            #region Auth
            CreateMap<LogoutRequest, Auth>().ReverseMap();
            CreateMap<LogoutRequest, Auth>().ConvertUsing(new NullValueIgnoringConverter<LogoutRequest, Auth>());
            #endregion

            #region Cart
            CreateMap<CreateCartRequest, Cart>().ReverseMap();
            CreateMap<CreateCartRequest, Cart>().ConvertUsing(new NullValueIgnoringConverter<CreateCartRequest, Cart>());

            CreateMap<CartItem, CartItemFullDTO>().ReverseMap();
            CreateMap<CartItem, CartItemFullDTO>().ConvertUsing(new NullValueIgnoringConverter<CartItem, CartItemFullDTO>());
            #endregion

            #region Category
            CreateMap<CreateCategoryRequest, Category>().ReverseMap();
            CreateMap<CreateCategoryRequest, Category>().ConvertUsing(new NullValueIgnoringConverter<CreateCategoryRequest, Category>());

            CreateMap<UpdateCategoryRequest, Category>().ReverseMap();
            CreateMap<UpdateCategoryRequest, Category>().ConvertUsing(new NullValueIgnoringConverter<UpdateCategoryRequest, Category>());
            #endregion

            #region Product
            CreateMap<CreateProductRequest, Product>().ReverseMap();
            CreateMap<CreateProductRequest, Product>().ConvertUsing(new NullValueIgnoringConverter<CreateProductRequest, Product>());

            CreateMap<UpdateProductRequest, Product>().ReverseMap();
            CreateMap<UpdateProductRequest, Product>().ConvertUsing(new NullValueIgnoringConverter<UpdateProductRequest, Product>());

            CreateMap<ProductFullDTO, Product>().ReverseMap();
            CreateMap<ProductFullDTO, Product>().ConvertUsing(new NullValueIgnoringConverter<ProductFullDTO, Product>());

            CreateMap<ProductFullDTO, ProductImageDTO>().ReverseMap();
            CreateMap<ProductFullDTO, ProductImageDTO>().ConvertUsing(new NullValueIgnoringConverter<ProductFullDTO, ProductImageDTO>());
            #endregion

            #region Product Detail
            CreateMap<CreateProductDetailRequest, ProductDetail>().ReverseMap();
            CreateMap<CreateProductDetailRequest, ProductDetail>().ConvertUsing(new NullValueIgnoringConverter<CreateProductDetailRequest, ProductDetail>());

            CreateMap<UpdateProductDetailRequest, ProductDetail>().ReverseMap();
            CreateMap<UpdateProductDetailRequest, ProductDetail>().ConvertUsing(new NullValueIgnoringConverter<UpdateProductDetailRequest, ProductDetail>());
            #endregion

            #region Product Variant
            CreateMap<CreateProductVariantRequest, ProductVariant>().ReverseMap();
            CreateMap<CreateProductVariantRequest, ProductVariant>().ConvertUsing(new NullValueIgnoringConverter<CreateProductVariantRequest, ProductVariant>());

            CreateMap<UpdateProductVariantRequest, ProductVariant>().ReverseMap();
            CreateMap<UpdateProductVariantRequest, ProductVariant>().ConvertUsing(new NullValueIgnoringConverter<UpdateProductVariantRequest, ProductVariant>());
            #endregion

            #region Product Image
            CreateMap<CreateProductImageRequest, ProductImage>().ReverseMap();
            CreateMap<UpdateProductImageRequest, ProductImage>().ConvertUsing(new NullValueIgnoringConverter<UpdateProductImageRequest, ProductImage>());

            CreateMap<ProductImage, ProductImageDTO>().ReverseMap();
            CreateMap<ProductImage, ProductImageDTO>().ConvertUsing(new NullValueIgnoringConverter<ProductImage, ProductImageDTO>());
            #endregion

            #region Attribute
            CreateMap<CreateAttributeRequest, Domain.Entities.ProductManagement.Attribute>().ReverseMap();
            CreateMap<CreateAttributeRequest, Domain.Entities.ProductManagement.Attribute>().ConvertUsing(new NullValueIgnoringConverter<CreateAttributeRequest, Domain.Entities.ProductManagement.Attribute>());

            CreateMap<UpdateAttributeRequest, Domain.Entities.ProductManagement.Attribute>().ReverseMap();
            CreateMap<UpdateAttributeRequest, Domain.Entities.ProductManagement.Attribute>().ConvertUsing(new NullValueIgnoringConverter<UpdateAttributeRequest, Domain.Entities.ProductManagement.Attribute>());
            #endregion

            #region Attribute Value
            CreateMap<CreateAttributeValueRequest, AttributeValue>().ReverseMap();
            CreateMap<CreateAttributeValueRequest, AttributeValue>().ConvertUsing(new NullValueIgnoringConverter<CreateAttributeValueRequest, AttributeValue>());

            CreateMap<UpdateAttributeValueRequest, AttributeValue>().ReverseMap();
            CreateMap<UpdateAttributeValueRequest, AttributeValue>().ConvertUsing(new NullValueIgnoringConverter<UpdateAttributeValueRequest, AttributeValue>());
            #endregion

            #region Coupon
            CreateMap<CreateCouponRequest, Coupon>().ReverseMap();
            CreateMap<CreateCouponRequest, Coupon>().ConvertUsing(new NullValueIgnoringConverter<CreateCouponRequest, Coupon>());

            CreateMap<UpdateCouponRequest, Coupon>().ReverseMap();
            CreateMap<UpdateCouponRequest, Coupon>().ConvertUsing(new NullValueIgnoringConverter<UpdateCouponRequest, Coupon>());

            CreateMap<CouponDTO, Coupon>().ReverseMap();
            #endregion

            #region Coupon Type
            CreateMap<CreateCouponTypeRequest, CouponType>().ReverseMap();
            CreateMap<CreateCouponTypeRequest, CouponType>().ConvertUsing(new NullValueIgnoringConverter<CreateCouponTypeRequest, CouponType>());

            CreateMap<UpdateCouponTypeRequest, CouponType>().ReverseMap();
            CreateMap<UpdateCouponTypeRequest, CouponType>().ConvertUsing(new NullValueIgnoringConverter<UpdateCouponTypeRequest, CouponType>());
            #endregion

            #region Promotion
            CreateMap<CreatePromotionRequest, Promotion>().ReverseMap();
            CreateMap<CreatePromotionRequest, Promotion>().ConvertUsing(new NullValueIgnoringConverter<CreatePromotionRequest, Promotion>());

            CreateMap<UpdatePromotionRequest, Promotion>().ReverseMap();
            CreateMap<UpdatePromotionRequest, Promotion>().ConvertUsing(new NullValueIgnoringConverter<UpdatePromotionRequest, Promotion>());
            #endregion

            #region Product Promotion
            CreateMap<UpdateProductPromotionRequest, ProductPromotion>().ReverseMap();
            CreateMap<UpdateProductPromotionRequest, ProductPromotion>().ConvertUsing(new NullValueIgnoringConverter<UpdateProductPromotionRequest, ProductPromotion>());
            #endregion

            #region User
            CreateMap<User, UpdateProfileUserRequest>().ReverseMap();
            CreateMap<User, UpdateProfileUserRequest>().ConvertUsing(new NullValueIgnoringConverter<User, UpdateProfileUserRequest>());
            #endregion

            #region User Address
            CreateMap<UserAddress, CreateUserAddressRequest>().ReverseMap();
            CreateMap<UserAddress, CreateUserAddressRequest>().ConvertUsing(new NullValueIgnoringConverter<UserAddress, CreateUserAddressRequest>());

            CreateMap<UserAddress, UpdateUserAddressRequest>().ReverseMap();
            CreateMap<UserAddress, UpdateUserAddressRequest>().ConvertUsing(new NullValueIgnoringConverter<UserAddress, UpdateUserAddressRequest>());
            #endregion

            #region Review
            CreateMap<CreateReviewRequest, Review>().ReverseMap();
            CreateMap<CreateReviewRequest, Review>().ConvertUsing(new NullValueIgnoringConverter<CreateReviewRequest, Review>());

            CreateMap<HandleReportRequest, Review>().ReverseMap();
            CreateMap<HandleReportRequest, Review>().ConvertUsing(new NullValueIgnoringConverter<HandleReportRequest, Review>());

            CreateMap<ReplyReviewByAdminRequest, ReviewReply>().ReverseMap();
            CreateMap<ReplyReviewByAdminRequest, ReviewReply>().ConvertUsing(new NullValueIgnoringConverter<ReplyReviewByAdminRequest, ReviewReply>());
            #endregion

            #region Review Report
            CreateMap<ReportReviewRequest, ReviewReport>().ReverseMap();
            CreateMap<ReportReviewRequest, ReviewReport>().ConvertUsing(new NullValueIgnoringConverter<ReportReviewRequest, ReviewReport>());

            CreateMap<HandleReportRequest, ReviewReport>().ReverseMap();
            CreateMap<HandleReportRequest, ReviewReport>().ConvertUsing(new NullValueIgnoringConverter<HandleReportRequest, ReviewReport>());

            CreateMap<ReportReviewRequest, ReviewReportUser>().ReverseMap();
            CreateMap<ReportReviewRequest, ReviewReportUser>().ConvertUsing(new NullValueIgnoringConverter<ReportReviewRequest, ReviewReportUser>());
            #endregion

            #region Return

            CreateMap<CreateReturnRequest, Return>().ReverseMap();
            CreateMap<CreateReturnRequest, Return>().ConvertUsing(new NullValueIgnoringConverter<CreateReturnRequest, Return>());

            CreateMap<RefundRequest, Return>().ReverseMap();
            CreateMap<RefundRequest, Return>().ConvertUsing(new NullValueIgnoringConverter<RefundRequest, Return>());

            #endregion

            #region Wish List

            CreateMap<CreateWishListRequest, WishList>().ReverseMap();
            CreateMap<CreateWishListRequest, WishList>().ConvertUsing(new NullValueIgnoringConverter<CreateWishListRequest, WishList>());

            #endregion

            #region Wish List

            CreateMap<CreateBannerRequest, Banner>().ReverseMap();
            CreateMap<CreateBannerRequest, Banner>().ConvertUsing(new NullValueIgnoringConverter<CreateBannerRequest, Banner>());

            CreateMap<UpdateBannerRequest, Banner>().ReverseMap();
            CreateMap<UpdateBannerRequest, Banner>().ConvertUsing(new NullValueIgnoringConverter<UpdateBannerRequest, Banner>());

            #endregion
        }
    }
}