using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CashFlowDailyBalance.Infra.CrossCutting.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5); // TTL de 5 minutos
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();

        public RedisCacheService()
        {
            _db = RedisConnectionFactory.GetDatabase();
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var value = await GetAsync<T>(key);
            if (value != null)
                return value;

            var semaphore = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
            
            try
            {
                await semaphore.WaitAsync();
                
                value = await GetAsync<T>(key);
                if (value != null)
                    return value;

                value = await factory();
                await SetAsync(key, value, expiration);
                return value;
            }
            finally
            {
                semaphore.Release();
                
              
                if (semaphore.CurrentCount == 1)
                {
                    _locks.TryRemove(key, out _);
                }
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var redisValue = await _db.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty)
                return default;

            return JsonConvert.DeserializeObject<T>(redisValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var redisValue = JsonConvert.SerializeObject(value);
            await _db.StringSetAsync(key, redisValue, expiration ?? _defaultExpiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }
    }
} 