using AppleShop.Share.Abstractions;
using AppleShop.Share.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace AppleShop.Share.Service
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer redis;
        private readonly IDatabase db;
        private readonly RedisSettings settings;

        public RedisCacheService(IConnectionMultiplexer redis, IOptions<RedisSettings> settings)
        {
            this.redis = redis;
            db = redis.GetDatabase();
            this.settings = settings.Value;
        }

        private IServer GetServer()
        {
            var endpoints = db.Multiplexer.GetEndPoints();
            return db.Multiplexer.GetServer(endpoints.First());
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await db.StringGetAsync(GetKey(key));
            if (value.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await db.StringSetAsync(GetKey(key), serializedValue, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            await db.KeyDeleteAsync(GetKey(key));
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var server = GetServer();
            if (!pattern.Contains("*")) pattern += "*";
            var fullPattern = GetKey(pattern);
            var keys = server.Keys(pattern: fullPattern);
            var redisKeys = keys.ToArray();
            if (redisKeys.Length > 0)
            {
                foreach (var k in redisKeys) System.Console.WriteLine($"[RedisCache] Xóa key: {k}");
                await db.KeyDeleteAsync(redisKeys);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await db.KeyExistsAsync(GetKey(key));
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
                return cachedValue;

            var value = await factory();
            await SetAsync(key, value, expiration);
            return value;
        }

        private string GetKey(string key)
        {
            return $"{settings.InstanceName}:{key}";
        }
    }
} 