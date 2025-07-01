using AppleShop.API.MinimalApis;

namespace AppleShop.API.DependencyInjection.Extension
{
    public static class EndpointExtensions
    {
        public static IEndpointRouteBuilder MapPresentationEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapControllers();

            #region Register Endpoint

            app.AuthAction();
            app.CartAction();
            app.CategoryAction();
            app.OrderAction();
            app.ProductAction();
            app.ProductImageAction();
            app.AttributeAction();
            app.CouponAction();
            app.UserAction();
            app.UserAddressAction();
            app.ProductVariantAction();
            app.AttributeValueAction();
            app.PromotionAction();
            app.ProductPromotionAction();
            app.ProductDetailAction();
            app.UserCouponAction();
            app.CouponTypeAction();
            app.ReviewAction();
            app.ReturnAction();
            app.WishListAction();
            app.BannerAction();
            app.AIPromptAction();
            app.StatisticalAction();
            app.SpinAction();
            app.ChatAction();

            #endregion

            return app;
        }
    }
}