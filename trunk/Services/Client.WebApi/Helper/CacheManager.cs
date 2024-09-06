using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Client.WebApi
{
    public class CacheManager<T>
    {
        private readonly IMemoryCache _memoryCache;
        public CacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetOrSetAsync(string cacheKey, Func<Task<T>> getDataFunc, TimeSpan expirationTime)
        {
            if (_memoryCache.TryGetValue(cacheKey, out T cachedData))
            {
                // Data found in cache, return it
                return cachedData;
            }
            else
            {
                // Data not found in cache, fetch asynchronously from the provided function
                T data = await getDataFunc();

                // Add data to cache with the specified expiration time, only if data is not null
                if (data != null)
                {
                    _memoryCache.Set(cacheKey, data, expirationTime);
                }

                return data;
            }
        }

        public async Task<List<T>> GetOrSetListAsync(string cacheKey, Func<Task<List<T>>> getDataFunc, TimeSpan expirationTime)
        {
            if (_memoryCache.TryGetValue(cacheKey, out List<T> cachedData))
            {
                // Data found in cache, return it
                return cachedData;
            }
            else
            {
                // Data not found in cache, fetch asynchronously from the provided function
                List<T> data = await getDataFunc();

                // Add data to cache with the specified expiration time, only if data is not null
                if (data != null)
                {
                    _memoryCache.Set(cacheKey, data, expirationTime);
                }

                return data;
            }
        }

        public async Task ClearCacheAndFetchDataAsync(string cacheKey, Func<Task<List<T>>> getDataFunc, TimeSpan expirationTime)
        {
            _memoryCache.Remove(cacheKey);

            // Fetch data from the provided function after clearing the cache
            List<T> data = await getDataFunc();

            // Add data to cache with the specified expiration time, only if data is not null
            if (data != null)
            {
                _memoryCache.Set(cacheKey, data, expirationTime);
            }
        }

        public void ClearCache(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }
    }
}
