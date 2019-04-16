using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Policy
{
    public class ThrottlePolicy : IThrottlePolicy
    {
        private readonly IThrottleRepository _repository;

        private List<Limiter> _limits;

        private string[] _prefixes;

        private string[] _IgnoreList;

        public ThrottlePolicy()
            : this(new MemoryThrottleRepository())
        {
        }

        public ThrottlePolicy(IThrottleRepository repository)
        {
            Limiters = new List<Limiter>();
            _repository = repository;
        }

        public long? PerSecond
        {
            get { return GetLimiterCount(TimeSpan.FromSeconds(1)); }
            set { SetLimiter(TimeSpan.FromSeconds(1), value); }
        }

        public long? PerMinute
        {
            get { return GetLimiterCount(TimeSpan.FromMinutes(1)); }
            set { SetLimiter(TimeSpan.FromMinutes(1), value); }
        }

        public long? PerHour
        {
            get { return GetLimiterCount(TimeSpan.FromHours(1)); }
            set { SetLimiter(TimeSpan.FromHours(1), value); }
        }

        public long? PerDay
        {
            get { return GetLimiterCount(TimeSpan.FromDays(1)); }
            set { SetLimiter(TimeSpan.FromDays(1), value); }
        }

        public ICollection<Limiter> Limiters
        {
            get { return _limits; }
            set { _limits = new List<Limiter>(value); }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 前缀
        /// </summary>
        public string[] Prefixes
        {
            get { return _prefixes; }
            set
            {
                _prefixes = value;
                _repository.PolicyIdentityValues = _prefixes;
            }
        }
        /// <summary>
        /// 忽略列表，可以是ip，或者请求地址等
        /// </summary>
        public string [] IgnoreList
        {
            get { return _IgnoreList; }
            set
            {
                _IgnoreList = value;
                _repository.IgnoreList = _IgnoreList;
            }
        }
        public bool IsThrottled(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsThrottled;
        }

        public bool IsLocked(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsLocked;
        }

        public CheckResult Check(IThrottleKey key, bool increment = true)
        {

            foreach (Limiter limiter in Limiters)
            {
                var result = new CheckResult
                {
                    IsThrottled = false,
                    IsLocked = false,
                    ThrottleKey = _repository.CreateThrottleKey(key, limiter),
                    Limiter = limiter
                };
                var ignorelist = _repository.SetOrGetIgnoreList();
                //判断是否在白名单中
                if (ignorelist.Length > 0)
                {
                    if (key.Values.Intersect(ignorelist).Count() > 0)
                    {
                        return result;
                    }
                }
                if (limiter.LockDuration.HasValue)
                {
                    result.LockKey = _repository.CreateLockKey(key, limiter);
                    if (_repository.LockExists(key, limiter))
                    {
                        result.IsLocked = true;
                        return result;
                    }
                }
                //判断有没有达到限制数量
                if (limiter.Count <= 0)
                    continue;

                long? counter = _repository.GetThrottleCount(key, limiter);
                //计数
                if (counter.HasValue
                    && counter.Value >= limiter.Count)
                {
                    if (limiter.LockDuration.HasValue)
                    {
                        //设置锁定
                        _repository.SetLock(key, limiter);
                        _repository.RemoveThrottle(key, limiter);
                    }

                    result.IsThrottled = true;
                    return result;
                }

                if (increment)
                    _repository.AddOrIncrementWithExpiration(key, limiter);
            }

            return CheckResult.NotThrottled;
        }

        private void SetLimiter(TimeSpan span, long? count)
        {
            Limiter item = Limiters.FirstOrDefault(l => l.Period == span);
            if (item != null)
                _limits.Remove(item);

            if (!count.HasValue)
                return;

            item = new Limiter
            {
                Count = count.Value,
                Period = span
            };

            _limits.Add(item);
        }

        private long? GetLimiterCount(TimeSpan span)
        {
            Limiter item = Limiters.FirstOrDefault(l => l.Period == span);
            long? result = null;

            if (item != null)
                result = item.Count;

            return result;
        }

        public bool Check(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsThrottled;
        }
    }
}
