using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Components
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly TimeSpan AbsoluteExpirationTime = TimeSpan.FromHours(24);
        private readonly TimeSpan SlidingExpirationTime = TimeSpan.FromHours(6);
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        //Accepts a string key and retrieves a cached item as a byte[] array if found in the cache.
        public T Get<T>(string key)
        {
            var value = _cache.GetString(key);
            if (value != null)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }
        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (value != null)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }
        //Adds an item (as byte[] array) to the cache using a string key.
        public T Set<T>(string key, T value)
        {
            var timeOut = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AbsoluteExpirationTime,
                SlidingExpiration = SlidingExpirationTime
            };
            _cache.SetString(key, JsonSerializer.Serialize(value), timeOut);
            return value;
        }
        public async Task<T> SetAsync<T>(string key, T value)
        {
            var timeOut = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AbsoluteExpirationTime,
                SlidingExpiration = SlidingExpirationTime
            };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), timeOut);
            return value;
        }
        //Refreshes an item in the cache based on its key, resetting its sliding expiration timeout (if any).
        public void Refresh(string key)
        {
            _cache.Refresh(key);
        }
        public async Task RefreshAsync(string key)
        {
            await _cache.RefreshAsync(key);
        }
        //Removes a cache item based on its string key.
        public void Remove(string key)
        {
            _cache.Remove(key);
        }
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
        public bool IsExits(string key)
        {
            var value = _cache.GetString(key);
            if (value != null)
                return true;
            else
                return false;
        }
        public async Task<bool> IsExitsAsync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (value != null)
                return true;
            else
                return false;
        }
        public bool ExistRemove(string key)
        {
            var value = _cache.GetString(key);
            if (value != null)
            {
                _cache.Remove(key);
                return true;
            }
            else
                return false;
        }
        public async Task<bool> ExistRemoveasync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (value != null)
            {
                _cache.Remove(key);
                return true;
            }
            else
                return false;
        }
    }
}
