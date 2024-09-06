using System.Threading.Tasks;

namespace Components
{
  
    public interface IRedisCacheService
    {
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        T Set<T>(string key, T value);
        Task<T> SetAsync<T>(string key, T value);
        void Refresh(string key);
        Task RefreshAsync(string key);
        void Remove(string key);
        Task RemoveAsync(string key);
        bool IsExits(string key);
        Task<bool> IsExitsAsync(string key);
        bool ExistRemove(string key);
        Task<bool> ExistRemoveasync(string key);
    }

}
