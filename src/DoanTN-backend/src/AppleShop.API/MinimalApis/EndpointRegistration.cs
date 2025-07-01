using AppleShop.Application.Requests.AIManagement.AIGenerate;
using AppleShop.Application.Requests.AIManagement.AIPrompt;
using AppleShop.Application.Requests.CategoryManagement.Category;
using AppleShop.Application.Requests.ChatManagement.Chat;
using AppleShop.Application.Requests.EventManagement.Banner;
using AppleShop.Application.Requests.EventManagement.Spin;
using AppleShop.Application.Requests.OrderManagement.Cart;
using AppleShop.Application.Requests.OrderManagement.OnlinePayment;
using AppleShop.Application.Requests.OrderManagement.Order;
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
using AppleShop.Application.Requests.StatisticalManagement.OrderReport;
using AppleShop.Application.Requests.StatisticalManagement.ProductReport;
using AppleShop.Application.Requests.StatisticalManagement.ReturnReport;
using AppleShop.Application.Requests.StatisticalManagement.SummaryReport;
using AppleShop.Application.Requests.StatisticalManagement.UserReport;
using AppleShop.Application.Requests.UserManagement.Admin;
using AppleShop.Application.Requests.UserManagement.Auth;
using AppleShop.Application.Requests.UserManagement.User;
using AppleShop.Application.Requests.UserManagement.UserAddress;
using AppleShop.Application.Requests.UserManagement.UserCoupon;
using AppleShop.Application.Requests.WishListManagement.WishList;
using AppleShop.Domain.Entities.UserManagement;
using AppleShop.Share.Constant;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AppleShop.API.MinimalApis
{
    public static class EndpointRegistration
    {
        #region Category API
        public static IEndpointRouteBuilder CategoryAction(this IEndpointRouteBuilder builder)
        {
            var category = builder.MapGroup("/api/v1/category").WithTags("Category V1");
            category.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateCategoryRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            category.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateCategoryRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            category.MapGet("/get-all", async ([FromQuery] int? isActived, IMediator mediator) =>
            {
                var query = new GetAllCategoriesRequest();
                query.IsActived = isActived;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            category.MapGet("/get-detail/{id}", async (int? id, IMediator mediator) =>
            {
                var query = new GetDetailCategoryRequest();
                query.Id = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Cart API
        public static IEndpointRouteBuilder CartAction(this IEndpointRouteBuilder builder)
        {
            var cart = builder.MapGroup("/api/v1/cart").WithTags("Cart V1");
            cart.MapPost("/create", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] CreateCartRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            cart.MapPut("/update-quantity", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] UpdateCartQuantityRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            cart.MapDelete("/delete-item/{variantId}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, int? variantId, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var command = new DeleteProductFromCartRequest();
                command.UserId = int.Parse(userIdClaim.Value);
                command.VariantId = variantId;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            cart.MapGet("/get-detail", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetDetailCartRequest();
                query.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Order API
        public static IEndpointRouteBuilder OrderAction(this IEndpointRouteBuilder builder)
        {
            var order = builder.MapGroup("/api/v1/order").WithTags("Order V1");
            order.MapPost("/create", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] CreateOrderRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            order.MapPut("/change-status/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] ChangeOrderStatusRequest command, IMediator mediator) =>
            {
                command.OiId = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            order.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (HttpContext httpContext, [FromQuery] int? status, [FromQuery] int? skip, [FromQuery] int? take, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetAllOrderRequest();
                query.UserId = int.Parse(userIdClaim.Value);
                query.Status = status;
                query.Skip = skip;
                query.Take = take;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            order.MapGet("/get-detail/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var query = new GetOrderDetailRequest();
                query.OrderId = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            order.MapGet("/view-history", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromQuery] int? status, [FromQuery] int? skip, [FromQuery] int? take, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new ViewPurchaseHistoryRequest();
                query.UserId = int.Parse(userIdClaim.Value);
                query.Status = status;
                query.Skip = skip;
                query.Take = take;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            order.MapGet("/get-detail/item/{oiId}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, int? oiId, IMediator mediator) =>
            {
                var query = new GetDetailItemRequest();
                query.OiId = oiId;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            order.MapGet("/payos/confirm/{orderCode}", async (string? orderCode, IMediator mediator) =>
            {
                var query = new PayOSConfirmRequest { OrderCode = orderCode };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Product API
        public static IEndpointRouteBuilder ProductAction(this IEndpointRouteBuilder builder)
        {
            var product = builder.MapGroup("/api/v1/product").WithTags("Product V1");
            product.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateProductRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            product.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateProductRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            product.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (IMediator mediator) =>
            {
                var query = new GetAllProductRequest();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            product.MapGet("/get-detail/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var query = new GetProductDetailRequest();
                query.Id = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Product Detail API
        public static IEndpointRouteBuilder ProductDetailAction(this IEndpointRouteBuilder builder)
        {
            var productDetail = builder.MapGroup("/api/v1/product-detail").WithTags("Product Detail V1");
            productDetail.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateProductDetailRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productDetail.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateProductDetailRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productDetail.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteProductDetailRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productDetail.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (IMediator mediator) =>
            {
                var command = new GetAllProductDetailRequest();
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Product Variant API
        public static IEndpointRouteBuilder ProductVariantAction(this IEndpointRouteBuilder builder)
        {
            var productVariant = builder.MapGroup("/api/v1/product-variant").WithTags("Product Variant V1");
            productVariant.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateProductVariantRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productVariant.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateProductVariantRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productVariant.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteProductVariantRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productVariant.MapGet("/get-all", async ([FromQuery] int? isActived,
                                                     [FromQuery] int? categoryId,
                                                     [FromQuery] decimal? minPrice,
                                                     [FromQuery] decimal? maxPrice,
                                                     [FromQuery] string? color,
                                                     [FromQuery] string? storage,
                                                     [FromQuery] bool? isDescending,
                                                     [FromQuery] bool? isAscending,
                                                     IMediator mediator,
                                                     [FromQuery] int? skip,
                                                     [FromQuery] int? take) =>
            {
                var query = new GetAllProductVariantRequest
                {
                    IsActived = isActived,
                    CategoryId = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Color = color,
                    Storage = storage,
                    IsDescending = isDescending,
                    IsAscending = isAscending,
                    Skip = skip,
                    Take = take
                };

                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            productVariant.MapGet("/get-detail/{id}", async (HttpContext httpContext, int? id, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                var query = new GetDetailProductVariantRequest();
                query.VariantId = id;
                if (userIdClaim is not null) query.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            productVariant.MapGet("/flash-sale", async (IMediator mediator) =>
            {
                var query = new GetFlashSaleRequest();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            productVariant.MapGet("/search", async (HttpContext httpContext, [FromQuery] string? name,
                                                     [FromQuery] int? skip,
                                                     [FromQuery] int? take,
                                                     IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                var query = new SearchProductRequest();
                query.Name = name;
                if (userIdClaim is not null) query.UserId = int.Parse(userIdClaim.Value);
                query.Skip = skip;
                query.Take = take;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            productVariant.MapGet("/color-options/{id}", async (int? id, IMediator mediator) =>
            {
                var query = new GetColorOptionsRequest();
                query.VariantId = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            productVariant.MapGet("/similar/{id}", async (int? id, [FromQuery] int? limit, IMediator mediator) =>
            {
                var query = new GetSimilarProductsRequest
                {
                    VariantId = id,
                    Limit = limit
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Product Image API
        public static IEndpointRouteBuilder ProductImageAction(this IEndpointRouteBuilder builder)
        {
            var productImage = builder.MapGroup("/api/v1/product-img").WithTags("Product Image V1");
            productImage.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateProductImageRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productImage.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateProductImageRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            productImage.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteProductImageRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Attribute API
        public static IEndpointRouteBuilder AttributeAction(this IEndpointRouteBuilder builder)
        {
            var attribute = builder.MapGroup("/api/v1/attribute").WithTags("Attribute V1");
            attribute.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateAttributeRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attribute.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateAttributeRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attribute.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteAttributeRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attribute.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (IMediator mediator) =>
            {
                var command = new GetAllAttributeRequest();
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attribute.MapGet("/get-detail/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new GetDetailAttributeRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Attribute Value API
        public static IEndpointRouteBuilder AttributeValueAction(this IEndpointRouteBuilder builder)
        {
            var attributeValue = builder.MapGroup("/api/v1/attribute-value").WithTags("Attribute Value V1");
            attributeValue.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateAttributeValueRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attributeValue.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateAttributeValueRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attributeValue.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteAttributeValueRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attributeValue.MapGet("/get-all", async (IMediator mediator) =>
            {
                var command = new GetAllAttributeValueRequest();
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            attributeValue.MapGet("/get-detail/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new GetDetailAttributeValueRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Coupon API
        public static IEndpointRouteBuilder CouponAction(this IEndpointRouteBuilder builder)
        {
            var coupon = builder.MapGroup("/api/v1/coupon").WithTags("Coupon V1");
            coupon.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateCouponRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            coupon.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateCouponRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            coupon.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteCouponRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            coupon.MapGet("/get-all", async (HttpContext httpContext, [FromQuery] bool? isActived, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                var query = new GetAllCouponRequest();
                query.IsActived = isActived;
                if (userIdClaim is not null) query.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            coupon.MapGet("/get-random", async (HttpContext httpContext, [FromQuery] int? count, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                var query = new GetRandomCouponsRequest();
                query.Count = count;
                if (userIdClaim is not null) query.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            coupon.MapGet("/get-detail/{code}", async (string? code, IMediator mediator) =>
            {
                var query = new GetDetailCouponRequest();
                query.Code = code;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Coupon Type API
        public static IEndpointRouteBuilder CouponTypeAction(this IEndpointRouteBuilder builder)
        {
            var couponType = builder.MapGroup("/api/v1/coupon-type").WithTags("Coupon Type V1");
            couponType.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateCouponTypeRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            couponType.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateCouponTypeRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            couponType.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteCouponTypeRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            couponType.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (IMediator mediator) =>
            {
                var query = new GetAllCouponTypeRequest();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Promotion API
        public static IEndpointRouteBuilder PromotionAction(this IEndpointRouteBuilder builder)
        {
            var promotion = builder.MapGroup("/api/v1/promotion").WithTags("Promotion V1");
            promotion.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreatePromotionRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            promotion.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdatePromotionRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            promotion.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeletePromotionRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            promotion.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] int? isActived, IMediator mediator) =>
            {
                var query = new GetAllPromotionRequest();
                query.IsActived = isActived;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            promotion.MapGet("/get-detail/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var query = new GetDetailPromotionRequest();
                query.Id = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Product Promotion API
        public static IEndpointRouteBuilder ProductPromotionAction(this IEndpointRouteBuilder builder)
        {
            var promotion = builder.MapGroup("/api/v1/product-promotion").WithTags("Product Promotion V1");
            promotion.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateProductPromotionRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            promotion.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateProductPromotionRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            promotion.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteProductPromotionRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            promotion.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (IMediator mediator) =>
            {
                var command = new GetAllProductPromotionRequest();
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            promotion.MapGet("/get-detail/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new GetDetailProductPromotionRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Return API
        public static IEndpointRouteBuilder ReturnAction(this IEndpointRouteBuilder builder)
        {
            var @return = builder.MapGroup("/api/v1/return").WithTags("Return V1");
            @return.MapPost("/create", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] CreateReturnRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            @return.MapPost("/refund", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] RefundRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            @return.MapPut("/change-status/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] ChangeStatusReturnRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            @return.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (HttpContext httpContext, [FromQuery] int? skip, [FromQuery] int? take,  IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetAllReturnRequest { UserId = int.Parse(userIdClaim.Value), Skip = skip, Take = take };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            @return.MapGet("/get-detail/item/{id}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var query = new GetDetailItemReturnRequest();
                query.ReturnId = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            @return.MapGet("/view-history", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromQuery] int? skip, [FromQuery] int? take, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new ViewReturnHistoryRequest { UserId = int.Parse(userIdClaim.Value), Skip = skip, Take = take };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Review API
        public static IEndpointRouteBuilder ReviewAction(this IEndpointRouteBuilder builder)
        {
            var review = builder.MapGroup("/api/v1/review").WithTags("Review V1");
            review.MapPost("/create", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] CreateReviewRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            review.MapPost("/reply", [Authorize(Roles = Const.ROLE_ADMIN)] async (HttpContext httpContext, [FromBody] ReplyReviewByAdminRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            review.MapGet("/get-all", async ([FromQuery] int? variantId,
                                             [FromQuery] int? star,
                                             [FromQuery] bool? isFilterByImage,
                                             [FromQuery] bool? isDeleted,
                                             [FromQuery] string? sortBy,
                                             [FromQuery] bool? isDescending,
                                             [FromQuery] int? pageNumber,
                                             [FromQuery] int? pageSize,
                                             IMediator mediator) =>
            {
                var query = new GetAllReviewRequest(new PaginationQuery
                {
                    SortBy = sortBy,
                    IsDescending = isDescending ?? true,
                    PageNumber = pageNumber ?? 1,
                    PageSize = pageSize ?? 10,
                })
                {
                    VariantId = variantId,
                    Star = star,
                    IsFilterByImage = isFilterByImage,
                    IsDeleted = isDeleted
                };

                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            review.MapGet("/get-all-by-admin", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] int? variantId, [FromQuery] int? star, IMediator mediator) =>
            {
                var query = new GetAllReviewByAdminRequest()
                {
                    VariantId = variantId,
                    Star = star
                };

                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            review.MapGet("/summary/{id}", async (int? id, IMediator mediator) =>
            {
                var query = new ReviewSumaryRequest();
                query.VariantId = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            review.MapPost("/report", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] ReportReviewRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            review.MapGet("/report/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (IMediator mediator) =>
            {
                var query = new GetAllReportRequest();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            review.MapPut("/report/handle/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] HandleReportRequest command, IMediator mediator) =>
            {
                command.ReportId = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Auth API
        public static IEndpointRouteBuilder AuthAction(this IEndpointRouteBuilder builder)
        {
            var auth = builder.MapGroup("/api/v1/auth").WithTags("Auth V1");
            auth.MapPost("/register", async ([FromBody] RegisterRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            auth.MapPut("/verify-otp", async ([FromBody] VerifyOTPRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            auth.MapPost("/verify-reset-password", async ([FromBody] VerifyResetPasswordRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            auth.MapPost("/resend-otp", async ([FromBody] ResendOTPRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            auth.MapGet("/login-google", async (HttpContext httpContext) =>
            {
                var properties = new AuthenticationProperties { RedirectUri = "/api/v1/auth/oauth-google" };
                await httpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
            });

            auth.MapGet("/oauth-google", async (HttpContext httpContext, IMediator mediator) =>
            {
                var loginResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!loginResult.Succeeded) return Results.Unauthorized();

                var principal = loginResult.Principal!;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value!;
                var userName = principal.FindFirst(ClaimTypes.Name)?.Value!;
                var picture = principal.FindFirst(Const.GOOGLE_PICTURE)?.Value!;

                var result = await mediator.Send(new GoogleLoginRequest
                {
                    Email = email,
                    UserName = userName,
                    ImageUrl = picture
                });

                if (result.IsSuccess)
                {
                    httpContext.Response.Cookies.Append(Const.REFRESH_TOKEN, result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.Now.AddDays(7)
                    });
                }

                //return Results.Redirect($"http://localhost:4200/auth/google?access_token={result.Data.AccessToken}");
                return Results.Redirect($"https://thh.id.vn/auth/google?access_token={result.Data.AccessToken}");
            });

            auth.MapGet("/login-facebook", async (HttpContext httpContext) =>
            {
                var properties = new AuthenticationProperties { RedirectUri = "/api/v1/auth/oauth-facebook" };
                await httpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme, properties);
            });

            auth.MapGet("/oauth-facebook", async (HttpContext httpContext, IMediator mediator) =>
            {
                var loginResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!loginResult.Succeeded) return Results.Unauthorized();

                var principal = loginResult.Principal!;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value!;
                var userName = principal.FindFirst(ClaimTypes.Name)?.Value!;

                var result = await mediator.Send(new FacebookLoginRequest
                {
                    Email = email,
                    UserName = userName
                });

                if (result.IsSuccess)
                {
                    httpContext.Response.Cookies.Append(Const.REFRESH_TOKEN, result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.Now.AddDays(7)
                    });
                }

                //return Results.Redirect($"http://localhost:4200/auth/facebook?{result.Data.AccessToken}");
                return Results.Redirect($"https://thh.id.vn/auth/facebook?{result.Data.AccessToken}");
            });

            auth.MapPost("/login", async (HttpContext httpContext, [FromBody] LoginRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                if (result.IsSuccess)
                {
                    httpContext.Response.Cookies.Append(Const.REFRESH_TOKEN, result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.Now.AddDays(7)
                    });
                }

                return Results.Ok(new { accessToken = result.Data.AccessToken });
            });

            auth.MapPost("/login-admin", async (HttpContext httpContext, [FromBody] AdminLoginRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                if (result.IsSuccess)
                {
                    httpContext.Response.Cookies.Append(Const.REFRESH_TOKEN, result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.Now.AddDays(7)
                    });
                }

                return Results.Ok(new { accessToken = result.Data.AccessToken });
            });

            auth.MapPost("/refresh-token", async (HttpContext httpContext, IMediator mediator) =>
            {
                var refreshToken = httpContext.Request.Cookies[Const.REFRESH_TOKEN];
                var result = await mediator.Send(new RefreshTokenRequest { RefreshToken = refreshToken });
                if (result.IsSuccess)
                {
                    httpContext.Response.Cookies.Append(Const.REFRESH_TOKEN, result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.Now.AddDays(7)
                    });

                    return Results.Ok(new { accessToken = result.Data.AccessToken });
                }
                else
                {
                    httpContext.Response.Cookies.Delete(Const.REFRESH_TOKEN, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                    });
                }

                return Results.Unauthorized();
            });

            auth.MapPost("/logout", async (HttpContext httpContext, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null)
                {
                    httpContext.Response.Cookies.Delete(Const.REFRESH_TOKEN, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                    });

                    return Results.Ok();
                }

                var command = new LogoutRequest { UserId = int.Parse(userIdClaim.Value) };
                var result = await mediator.Send(command);
                httpContext.Response.Cookies.Delete(Const.REFRESH_TOKEN, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                });

                return Results.Ok(result);
            });

            auth.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            auth.MapPut("/change-password", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, ChangePasswordRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region User Coupon API
        public static IEndpointRouteBuilder UserCouponAction(this IEndpointRouteBuilder builder)
        {
            var userCoupon = builder.MapGroup("/api/v1/user-coupon").WithTags("User Coupon V1");
            userCoupon.MapPost("/create", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] CreateUserCouponRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            userCoupon.MapGet("/get-by-user-id", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetCouponByUserIdRequest();
                query.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region User API
        public static IEndpointRouteBuilder UserAction(this IEndpointRouteBuilder builder)
        {
            var user = builder.MapGroup("/api/v1/user").WithTags("User V1");
            user.MapPut("/update-profile", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] UpdateProfileUserRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.Id = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            user.MapGet("/get-profile", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetProfileUserRequest();
                query.Id = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            user.MapPut("/change-status/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] ChangeStatusUserRequest command, IMediator mediator) =>
            {
                command.UserId = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            user.MapGet("/get-all", [Authorize(Roles = Const.ROLE_ADMIN)] async (HttpContext httpContext, [FromQuery] int? role, [FromQuery] int? isActived, [FromQuery] int? skip, [FromQuery] int? take, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetAllUserRequest
                {
                    Role = role,
                    IsActived = isActived,
                    Skip = skip,
                    Take = take
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            user.MapGet("/search", [Authorize(Roles = Const.ROLE_ADMIN)] async (HttpContext httpContext, [FromQuery] string? email, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var command = new SearchUserRequest
                {
                    Email = email,
                };
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region User Address API
        public static IEndpointRouteBuilder UserAddressAction(this IEndpointRouteBuilder builder)
        {
            var userAddress = builder.MapGroup("/api/v1/user-address").WithTags("User Address V1");
            userAddress.MapPost("/create", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] CreateUserAddressRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            userAddress.MapGet("/provinces", async (IHttpClientFactory httpClientFactory) =>
            {
                var httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync("https://provinces.open-api.vn/api/p/");

                if (!response.IsSuccessStatusCode)
                    return Results.Problem("Không thể truy cập API tỉnh thành.");

                var content = await response.Content.ReadAsStringAsync();
                return Results.Content(content, "application/json");
            });

            userAddress.MapGet("/districts/{provinceId}", async (int provinceId, IHttpClientFactory factory) =>
            {
                var client = factory.CreateClient();
                var url = $"https://provinces.open-api.vn/api/p/{provinceId}?depth=2";
                var res = await client.GetAsync(url);
                var content = await res.Content.ReadAsStringAsync();
                return Results.Content(content, "application/json");
            });

            userAddress.MapGet("/wards/{districtId}", async (int districtId, IHttpClientFactory factory) =>
            {
                var client = factory.CreateClient();
                var url = $"https://provinces.open-api.vn/api/d/{districtId}?depth=2";
                var res = await client.GetAsync(url);
                var content = await res.Content.ReadAsStringAsync();
                return Results.Content(content, "application/json");
            });

            userAddress.MapPut("/update/{id}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (int? id, [FromBody] UpdateUserAddressRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            userAddress.MapPut("/set-default/{id}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (int? id, [FromBody] SetDefaultAddressRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            userAddress.MapDelete("/delete/{id}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteUserAddressRequest();
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            userAddress.MapGet("/get-all", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetAddressByUserIdRequest { UserId = int.Parse(userIdClaim.Value) };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            userAddress.MapGet("/get-detail/{id}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var query = new GetDetailUserAddressRequest();
                query.Id = id;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Wish List

        public static IEndpointRouteBuilder WishListAction(this IEndpointRouteBuilder builder)
        {
            var wishListV1 = builder.MapGroup("/api/v1/wish-list").WithTags("Wish List V1");
            wishListV1.MapPost("/create", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, [FromBody] CreateWishListRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            wishListV1.MapPut("/change-status/{wishListId}", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, int? wishListId, [FromBody] ChangeStatusWishListRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                command.VariantId = wishListId;
                command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            wishListV1.MapGet("/get-detail", [Authorize(Policy = Const.ROLE_USER_OR_ADMIN)] async (HttpContext httpContext, bool? isActived, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is null) return Results.Unauthorized();

                var query = new GetDetailWishListRequest
                {
                    UserId = int.Parse(userIdClaim.Value),
                    IsActived = isActived
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }

        #endregion

        #region Banner

        public static IEndpointRouteBuilder BannerAction(this IEndpointRouteBuilder builder)
        {
            var bannerV1 = builder.MapGroup("/api/v1/banner").WithTags("Banner V1");
            bannerV1.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreateBannerRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            bannerV1.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdateBannerRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            bannerV1.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeleteBannerRequest { Id = id };
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            bannerV1.MapGet("/get-all", async (HttpContext httpContext, int? status, int? position, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var query = new GetAllBannerRequest
                {
                    Status = status,
                    Position = position
                };
                if (userIdClaim is not null) query.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }

        #endregion

        #region AI Prompt

        public static IEndpointRouteBuilder AIPromptAction(this IEndpointRouteBuilder builder)
        {
            var aiPrompt = builder.MapGroup("/api/v1/prompt").WithTags("AI Prompt V1");
            aiPrompt.MapPost("/create", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] CreatePromptRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            aiPrompt.MapPost("/generate-description", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] ProductDescriptionGenerateRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            aiPrompt.MapPut("/update/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, [FromBody] UpdatePromptRequest command, IMediator mediator) =>
            {
                command.Id = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            aiPrompt.MapDelete("/delete/{id}", [Authorize(Roles = Const.ROLE_ADMIN)] async (int? id, IMediator mediator) =>
            {
                var command = new DeletePromptRequest { Id = id };
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            aiPrompt.MapGet("/get-all", async (IMediator mediator) =>
            {
                var query = new GetAllPromptRequest();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }

        #endregion

        #region Statistical API
        public static IEndpointRouteBuilder StatisticalAction(this IEndpointRouteBuilder builder)
        {
            var report = builder.MapGroup("/api/v1/statistical").WithTags("Statistical V1");

            report.MapGet("/users/new", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, IMediator mediator) =>
            {
                var query = new GetNewUserReportRequest
                {
                    StartDate = startDate,
                    EndDate = endDate
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            report.MapGet("/users/top-spending", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? topCount, IMediator mediator) =>
            {
                var query = new GetTopSpendingUsersRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TopCount = topCount.Value
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            report.MapGet("/products/top", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? limit, [FromQuery] string? type, IMediator mediator) =>
            {
                var query = new GetTopProductReportRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Limit = limit,
                    Type = type
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            report.MapGet("/orders", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? timeUnit, IMediator mediator) =>
            {
                var query = new GetOrderReportRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeUnit = timeUnit
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            report.MapGet("/returns", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? timeUnit, [FromQuery] int? topReasonsCount, IMediator mediator) =>
            {
                var query = new GetReturnReportRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeUnit = timeUnit,
                    TopReasonsCount = topReasonsCount
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            report.MapGet("/summary", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? timeUnit, IMediator mediator) =>
            {
                var query = new GetSummaryReportRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeUnit = timeUnit
                };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion

        #region Banner

        public static IEndpointRouteBuilder SpinAction(this IEndpointRouteBuilder builder)
        {
            var spin = builder.MapGroup("/api/v1/spin").WithTags("Spin V1");
            spin.MapPost("/save-history", async (HttpContext httpContext, [FromBody] SaveSpinHistoryRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is not null) command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            spin.MapGet("/check-today", async (HttpContext httpContext, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var query = new CheckSpinTodayRequest();
                if (userIdClaim is not null) query.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            return builder;
        }

        #endregion

        #region Chat API
        public static IEndpointRouteBuilder ChatAction(this IEndpointRouteBuilder builder)
        {
            var chat = builder.MapGroup("/api/v1/chat").WithTags("Chat V1");

            chat.MapPost("/message", async (HttpContext httpContext, [FromBody] SendChatMessageRequest command, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim is not null) command.UserId = int.Parse(userIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            chat.MapGet("/conversation", async (HttpContext httpContext, [FromQuery] string? status, IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                var query = new GetConversationRequest();
                if (userIdClaim is not null) query.UserId = int.Parse(userIdClaim.Value);
                else query.Status = status;
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            chat.MapGet("/admin/pending", [Authorize(Roles = Const.ROLE_ADMIN)] async (IMediator mediator) =>
            {
                var query = new GetPendingConversationsRequest();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            });

            chat.MapPost("/admin/reply", [Authorize(Roles = Const.ROLE_ADMIN)] async (HttpContext httpContext, [FromBody] AdminReplyRequest command, IMediator mediator) =>
            {
                var adminIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (adminIdClaim is null) return Results.Unauthorized();

                command.AdminId = int.Parse(adminIdClaim.Value);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            chat.MapPut("/bot/toggle", [Authorize(Roles = Const.ROLE_ADMIN)] async ([FromBody] ToggleBotRequest command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            return builder;
        }
        #endregion
    }
}