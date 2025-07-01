namespace AppleShop.Share.Settings
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int DefaultDatabase { get; set; }
        public int ConnectTimeout { get; set; }
        public bool AbortOnConnectFail { get; set; }
    }
} 