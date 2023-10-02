using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.Utils.Utils.MemCache
{
    public class MemCache<T> : IMemCache<T>
    {
        private static volatile Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache((IOptions<MemoryCacheOptions>)new MemoryCacheOptions());

        /// <summary>
        /// 验证缓存项是否存在
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return _memoryCache.TryGetValue(key, out _);
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresIn">缓存时间</param>
        /// <returns></returns>
        public void Set(string key, T value, int expiresIn)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (expiresIn > 0)
            {
                _memoryCache.Set<T>(key, value,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(120))
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(expiresIn)));
            }
            else
            {
                _memoryCache.Set<T>(key, value,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(120))
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(1440)));
            }
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _memoryCache.Remove(key);
        }

        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <returns></returns>
        public void RemoveAll(IEnumerable<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            keys.ToList().ForEach(item => _memoryCache.Remove(item));
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public T? Get(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var res = _memoryCache.Get<T>(key);
            return res;
        }


        public T? Get(string key, T? defaultValue)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var res = _memoryCache.Get<T>(key) ?? defaultValue;
            return res;
        }


        /// <summary>
        /// 获取缓存集合
        /// </summary>
        /// <param name="keys">缓存Key集合</param>
        /// <returns></returns>
        public IDictionary<string, T?> GetAll(IEnumerable<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var dict = new Dictionary<string, T?>();
            keys.ToList().ForEach(key => {
                T? item = _memoryCache.Get<T>(key);
                dict.Add(key, item);
            });
            return dict;
        }


        /// <summary>
        /// 删除所有缓存
        /// </summary>
        public void RemoveAll()
        {
            var l = GetCacheKeys();
            foreach (var s in l)
            {
                Remove(s);
            }
        }

        /// <summary>
        /// 删除匹配到的缓存
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public void RemoveCacheRegex(string pattern)
        {
            IList<string> l = SearchCacheRegex(pattern);
            foreach (var s in l)
            {
                Remove(s);
            }
        }

        /// <summary>
        /// 搜索 匹配到的缓存
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public IList<string> SearchCacheRegex(string pattern)
        {
            var cacheKeys = GetCacheKeys();
            var l = cacheKeys.Where(k => Regex.IsMatch(k, pattern)).ToList();
            return l.AsReadOnly();
        }

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns></returns>
        public List<string> GetCacheKeys()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _memoryCache.GetType().GetField("_entries", flags)?.GetValue(_memoryCache);
            var keys = new List<string>();
            if (entries is not IDictionary cacheItems) 
                return keys;
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                string? keyStr = cacheItem.Key.ToString();
                if(keyStr is not null)
                    keys.Add(keyStr);
            }
            return keys;
        }
    }
}
