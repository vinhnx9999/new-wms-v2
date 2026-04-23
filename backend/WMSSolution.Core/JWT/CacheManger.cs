using Microsoft.Extensions.Caching.Memory;

namespace WMSSolution.Core.JWT
{
    /// <summary>
    /// 
    /// </summary>
    public class CacheManager
    {
        /// <summary>
        /// 
        /// </summary>
        public static CacheManager Default = new CacheManager();

        private IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 
        /// </summary>
        public CacheManager()
        {

        }

        /// <summary>
        /// get value by key
        /// </summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        public T? Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            T value;
            _cache.TryGetValue<T>(key, out value!);
            return value;
        }


        /// <summary>
        /// set cache
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Set_NotExpire<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            T v;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);
            _cache.Set(key, value);
        }

        /// <summary>
        /// set cache with expire
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Set_SlidingExpire<T>(string key, T value, TimeSpan span)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            T v;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);
            _cache.Set(key, value, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = span
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="span"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Set_AbsoluteExpire<T>(string key, T value, TimeSpan span)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            T v;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);
            _cache.Set(key, value, span);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingSpan"></param>
        /// <param name="absoluteSpan"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Set_SlidingAndAbsoluteExpire<T>(string key, T value, TimeSpan slidingSpan, TimeSpan absoluteSpan)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            T v;
            if (_cache.TryGetValue(key, out v))
                _cache.Remove(key);
            _cache.Set(key, value, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = slidingSpan,
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(absoluteSpan.TotalMilliseconds)
            });
        }

        /// <summary>
        /// remove cache by key
        /// </summary> 
        /// <param name="key">key</param>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _cache.Remove(key);
        }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            if (_cache != null)
                _cache.Dispose();
            GC.SuppressFinalize(this);
        }

        #region TokenHelper
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userID"></param>
        /// <param name="type"></param>
        /// <param name="expireMinute"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Is_Token_Exist<T>(int userID, string type, int expireMinute)
        {
            var key = $"WMSSolution_{type}_{userID}";
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            T value;
            if (_cache.TryGetValue<T>(key, out value))
            {
                Set_SlidingExpire(key, value, TimeSpan.FromMinutes(expireMinute));
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="type"></param>
        /// <param name="token"></param>
        /// <param name="expireMinute"></param>
        /// <returns></returns>
        public async Task<bool> TokenSet(int userID, string type, string token, int expireMinute)
        {
            string key = $"WMSSolution_{type}_{userID}";
            try
            {
                Set_AbsoluteExpire(key, token, TimeSpan.FromMinutes(expireMinute));
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}

