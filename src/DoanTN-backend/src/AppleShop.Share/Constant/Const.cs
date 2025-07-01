namespace AppleShop.Share.Constant
{
    public class Const
    {
        #region Database and Server

        public const string CONN_CONFIG_SQL = "DbSqlServer";
        public const string ASSET_SERVER_ADDRESS = "AssetsService:AssetServer";

        #endregion

        #region Rabbit MQ

        public const string BROKER_CONFIG = "MessageBroker";
        public const string BROKER_HOST = "Host";
        public const string BROKER_USERNAME = "Username";
        public const string BROKER_PASSWORD = "Password";

        #endregion

        #region JWT

        public const string AUTHEN_KEY = "Authentication:Key";
        public const string ROLE_USER = "0";
        public const string ROLE_ADMIN = "1";
        public const string ROLE_USER_OR_ADMIN = "UserOrAdmin";

        #endregion

        #region Gateway

        public const string REVERSE_PROXY = "ReverseProxy";

        #endregion

        #region Cookie

        public const string REFRESH_TOKEN = "refresh_token";
        public const string ORDER_CODE = "order_code";
        public const string COOKIES = "Cookies";

        #endregion

        #region Config

        public const string REQUEST_CONFIG = "RequestConfig";

        #endregion

        #region Hangfire

        public const string HANGFIRE_ROUTE = "/hangfire";
        public const string CLEAN_VOUCHER_JOB = "cleanup-expired-vouchers";
        public const string CANCEL_UNPAID_ORDERS = "cancel-unpaid-orders";

        #endregion

        #region Setting

        public const string HOST_SETTING = "HostSetting";

        #endregion

        #region Google

        public const string GOOGLE_EXTERNAL = "External";
        public const string GOOGLE = "Google";
        public const string GOOGLE_CLIENT_ID = "Authentication:Google:ClientId";
        public const string GOOGLE_CLIENT_SECRET = "Authentication:Google:ClientSecret";
        public const string GOOGLE_PICTURE = "urn:google:picture";
        public const string PICTURE = "picture";
        public const string URL = "url";

        #endregion

        #region Facebook

        public const string FACEBOOK_APP_ID = "Authentication:Facebook:AppId";
        public const string FACEBOOK_SECRET_KEY = "Authentication:Facebook:AppSecret";
        public const string EMAIL = "email";
        public const string NAME = "name";

        #endregion

        #region PayOS

        public const string PAYOS_CLIENT_ID = "PayOS:ClientId";
        public const string PAYOS_API_KEY = "PayOS:ApiKey";
        public const string PAYOS_CHECKSUM_KEY = "PayOS:ChecksumKey";
        public const string PAYOS_CANCEL_URL = "PayOS:CancelUrl";
        public const string PAYOS_RETURN_URL = "PayOS:ReturnUrl";

        #endregion
    }
}