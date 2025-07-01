using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Domain.Abstractions.IRepositories.Base;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.PromotionManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Abstractions.IRepositories.WishListManagement;
using AppleShop.Infrastructure.Repositories.AIManagement;
using AppleShop.Infrastructure.Repositories.Base;
using AppleShop.Infrastructure.Repositories.CategoryManagement;
using AppleShop.Infrastructure.Repositories.ChatManagement;
using AppleShop.Infrastructure.Repositories.EventManagement;
using AppleShop.Infrastructure.Repositories.OrderManagement;
using AppleShop.Infrastructure.Repositories.ProductManagement;
using AppleShop.Infrastructure.Repositories.PromotionManagement;
using AppleShop.Infrastructure.Repositories.ReturnManagement;
using AppleShop.Infrastructure.Repositories.ReviewManagement;
using AppleShop.Infrastructure.Repositories.UserManagement;
using AppleShop.Infrastructure.Repositories.WishListManagement;
using AppleShop.Share.Constant;
using AppleShop.Share.DependencyInjection.Extensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppleShop.Infrastructure.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(Const.CONN_CONFIG_SQL);
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();
            services.AddConfigSetting(configuration);
            services.AddRedisCache(configuration);
            services.RegisterServices();
            return services;
        }

        private static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            #region AIManagement

            services.AddScoped<IAIPromptRepository, AIPromptRepository>();
            services.AddScoped<IUserInteractionRepository, UserInteractionRepository>();

            #endregion

            #region Base

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            #endregion

            #region CategoryManagement

            services.AddScoped<ICategoryRepository, CategoryRepository>();

            #endregion

            #region ChatManagement

            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

            #endregion

            #region EventManagement

            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<ISpinHistoryRepository, SpinHistoryRepository>();

            #endregion

            #region OrderManagement

            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            #endregion

            #region ProductManagement

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductDetailRepository, ProductDetailRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IAttributeRepository, AttributeRepository>();
            services.AddScoped<IAttributeValueRepository, AttributeValueRepository>();
            services.AddScoped<IProductViewRepository, ProductViewRepository>();

            #endregion

            #region PromotionManagement

            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IProductPromotionRepository, ProductPromotionRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<ICouponTypeRepository, CouponTypeRepository>();

            #endregion

            #region ReturnManagement

            services.AddScoped<IReturnRepository, ReturnRepository>();

            #endregion

            #region ReviewManagement

            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IReviewMediaRepository, ReviewMediaRepository>();
            services.AddScoped<IReviewReplyRepository, ReviewReplyRepository>();
            services.AddScoped<IReviewReportRepository, ReviewReportRepository>();
            services.AddScoped<IReviewReportUserRepository, ReviewReportUserRepository>();

            #endregion

            #region UserManagement

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();
            services.AddScoped<IUserCouponRepository, UserCouponRepository>();

            #endregion

            #region WishListManagement

            services.AddScoped<IWishListRepository, WishListRepository>();

            #endregion

            return services;
        }
    }
}