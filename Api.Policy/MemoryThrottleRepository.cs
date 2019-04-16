using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api.Policy
{
    public class MemoryThrottleRepository : IThrottleRepository
    {
        /// <summary>
        /// 使用缓存
        /// </summary>
        static MemoryCache _store;

        // 
        public Func<DateTime> CurrentDate = () => DateTime.UtcNow;

        public MemoryThrottleRepository(MemoryCache cache)
        {
            _store = cache;
        }

        public MemoryThrottleRepository()
        {
            _store = new MemoryCache(new MemoryCacheOptions());
        }

        public object[] PolicyIdentityValues { get; set; }
        public object[] IgnoreList { get; set; }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public long? GetThrottleCount(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);

            var cacheItem = _store.Get(id) as ThrottleCacheItem;
            if (cacheItem != null)
            {
                return cacheItem.Count;
            }

            return null;
        }

        public void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);
            var cacheItem = _store.Get(id) as ThrottleCacheItem;

            if (cacheItem != null)
            {
                cacheItem.Count = cacheItem.Count + 1;
                Console.WriteLine(cacheItem.Count);
            }
            else
            {
                cacheItem = new ThrottleCacheItem()
                {
                    Count = 1,
                    Expiration = CurrentDate().Add(limiter.Period)
                };
            }

            _store.Set(id, cacheItem, cacheItem.Expiration);
        }

        public void SetLock(IThrottleKey key, Limiter limiter)
        {
            string throttleId = CreateThrottleKey(key, limiter);
            _store.Remove(throttleId);

            string lockId = CreateLockKey(key, limiter);
            DateTime expiration = CurrentDate().Add(limiter.LockDuration.Value);
            _store.Set(lockId, true, expiration);
        }

        public bool LockExists(IThrottleKey key, Limiter limiter)
        {
            string lockId = CreateLockKey(key, limiter);
            object reslut = null;
            return _store.TryGetValue(lockId, out reslut);
        }

        public void RemoveThrottle(IThrottleKey key, Limiter limiter)
        {
            string lockId = CreateThrottleKey(key, limiter);
            _store.Remove(lockId);
        }

        public string CreateLockKey(IThrottleKey key, Limiter limiter)
        {
            List<object> values = CreateBaseKeyValues(key, limiter);

            string lockKeySuffix = TimeSpanToFriendlyString(limiter.LockDuration.Value);
            values.Add("lock");
            values.Add(lockKeySuffix);

            string id = string.Join(":", values);
            return id;
        }

        public string CreateThrottleKey(IThrottleKey key, Limiter limiter)
        {
            List<object> values = CreateBaseKeyValues(key, limiter);

            string countKey = TimeSpanToFriendlyString(limiter.Period);
            values.Add(countKey);

            //使用unix时间戳
            if (limiter.Period.TotalSeconds == 1)
                values.Add(GetUnixTimestamp());

            string id = string.Join(":", values);
            return id;
        }

        private List<object> CreateBaseKeyValues(IThrottleKey key, Limiter limiter)
        {
            List<object> values = key.Values.ToList();
            if (PolicyIdentityValues != null && PolicyIdentityValues.Length > 0)
                values.InsertRange(0, PolicyIdentityValues);

            return values;
        }

        private string TimeSpanToFriendlyString(TimeSpan span)
        {
            var items = new List<string>();
            Action<double, string> ifNotZeroAppend = (value, key) =>
            {
                if (value != 0)
                    items.Add(string.Concat(value, key));
            };

            ifNotZeroAppend(span.Days, "d");
            ifNotZeroAppend(span.Hours, "h");
            ifNotZeroAppend(span.Minutes, "m");
            ifNotZeroAppend(span.Seconds, "s");

            return string.Join("", items);
        }

        private long GetUnixTimestamp()
        {
            TimeSpan timeSpan = (CurrentDate() - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        public string[] SetOrGetIgnoreList()
        {
            string[] outreslut;
            var hasvalue = _store.TryGetValue("IgnoreList", out outreslut);
            if (hasvalue)
            {
                return outreslut;
            }
            else
            {
                _store.Set("IgnoreList", (string[])IgnoreList);
                _store.TryGetValue("IgnoreList", out outreslut);
            }
            return outreslut;
        }

        [Serializable]
        public class ThrottleCacheItem
        {
            public long Count { get; set; }

            public DateTime Expiration { get; set; }
        }
    }
}
