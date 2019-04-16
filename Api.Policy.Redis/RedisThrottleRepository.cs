using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSRedis;

namespace Api.Policy.Redis
{
    public class RedisThrottleRepository : IThrottleRepository
    {
        private static CSRedisClient CSRedis;
        private string _ConnectionStrings;
        public RedisThrottleRepository(string Connectionstring)
        {
            _ConnectionStrings = Connectionstring;
            CSRedis = new CSRedisClient(_ConnectionStrings);
        }
        public object[] PolicyIdentityValues { get; set; }
        public object[] IgnoreList { get; set; }

        public long? GetThrottleCount(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);
            var value = CSRedis.Get(id);
            long convert;
            if (long.TryParse(value, out convert))
                return convert;

            return null;
        }
        /// <summary>
        /// 设置忽略的IP列表
        /// </summary>
        /// <param name="IPaddress"></param>
        public string[] SetOrGetIgnoreList()
        {
            var reslut= CSRedis.StartPipe()
                .Set("IgnoreList", IgnoreList)
                .Get<string[]>("IgnoreList")
                .EndPipe().LastOrDefault();
            return (string[])reslut;
        }
        public void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);

            //设置增量
            long result = CSRedis.IncrBy(id);
            //设置过期时间
            if (result == 1)
                CSRedis.Expire(id, limiter.Period);
        }
        /// <summary>
        /// 判断锁定是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public bool LockExists(IThrottleKey key, Limiter limiter)
        {
            string id = CreateLockKey(key, limiter);
            return CSRedis.Exists(id);
        }
        /// <summary>
        /// 设置锁定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void SetLock(IThrottleKey key, Limiter limiter)
        {
            string id = CreateLockKey(key, limiter);

            CSRedis.StartPipe()
                .IncrBy(id, 1)
                .Expire(id, limiter.LockDuration.Value)
                .EndPipe();
        }
        /// <summary>
        /// 移除限制状态
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void RemoveThrottle(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);
            CSRedis.Del(id);
        }
        /// <summary>
        /// 生成key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public string CreateThrottleKey(IThrottleKey key, Limiter limiter)
        {
            List<object> values = CreateBaseKeyValues(key, limiter);

            string countKey = TimeSpanToFriendlyString(limiter.Period);
            values.Add(countKey);

            //使用unix时间戳提高精度
            if (limiter.Period.TotalSeconds == 1)
                values.Add(GetUnixTimestamp());

            string id = string.Join(":", values);
            return id;
        }
        /// <summary>
        /// 生成锁定状态的key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public string CreateLockKey(IThrottleKey key, Limiter limiter)
        {
            List<object> values = CreateBaseKeyValues(key, limiter);

            string lockKeySuffix = TimeSpanToFriendlyString(limiter.LockDuration.Value);
            values.Add("lock");
            values.Add(lockKeySuffix);

            string id = string.Join(":", values);
            return id;
        }
        /// <summary>
        /// 创建key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取nunix时间戳
        /// </summary>
        /// <returns></returns>
        private long GetUnixTimestamp()
        {
            TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
    }
}
