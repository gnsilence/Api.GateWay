using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Policy
{
  public  class Limiter
    {
        /// <summary>
        /// 数量
        /// </summary>
        public long Count { get; set; }
        /// <summary>
        /// 限制时长
        /// </summary>
        public TimeSpan Period { get; set; }
        /// <summary>
        /// 锁定时长
        /// </summary>
        public TimeSpan? LockDuration { get; set; }

        public string [] IgnoreList { get; set; }

        public Limiter Limit(long count)
        {
            Count = count;
            return this;
        }

        public Limiter Over(long seconds)
        {
            return Over(TimeSpan.FromSeconds(seconds));
        }

        public Limiter Over(TimeSpan span)
        {
            Period = span;
            return this;
        }
        /// <summary>
        /// 每秒
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Limiter PerSecond(long count)
        {
            return Limit(count).Over(1);
        }

        public Limiter PerMinute(long count)
        {
            return Limit(count).Over(60);
        }

        public Limiter PerHour(long count)
        {
            return Limit(count).Over(TimeSpan.FromHours(1));
        }

        public Limiter PerDay(long count)
        {
            return Limit(count).Over(TimeSpan.FromDays(1));
        }
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public Limiter LockFor(long seconds)
        {
            return LockFor(TimeSpan.FromSeconds(seconds));
        }

        public Limiter LockFor(TimeSpan span)
        {
            LockDuration = span;
            return this;
        }
    }
}
