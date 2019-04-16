using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Policy.Sqlite
{
    public class SqliteThrottleRepository : IThrottleRepository
    {
        public object[] PolicyIdentityValues { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object[] IgnoreList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter)
        {
            throw new NotImplementedException();
        }

        public string CreateLockKey(IThrottleKey key, Limiter limiter)
        {
            throw new NotImplementedException();
        }

        public string CreateThrottleKey(IThrottleKey key, Limiter limiter)
        {
            throw new NotImplementedException();
        }

        public long? GetThrottleCount(IThrottleKey key, Limiter limiter)
        {
            throw new NotImplementedException();
        }

        public bool LockExists(IThrottleKey key, Limiter limiter)
        {
            throw new NotImplementedException();
        }

        public void RemoveThrottle(IThrottleKey key, Limiter limiter)
        {
            throw new NotImplementedException();
        }

        public void SetLock(IThrottleKey key, Limiter limiter)
        {
            throw new NotImplementedException();
        }

        public string[] SetOrGetIgnoreList()
        {
            throw new NotImplementedException();
        }
    }
}
