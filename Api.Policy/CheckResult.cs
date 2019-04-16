using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Policy
{
  public  class CheckResult
    {
        /// <summary>
        /// 限制状态
        /// </summary>
        public static readonly CheckResult NotThrottled = new CheckResult { IsThrottled = false, IsLocked = false };
        /// <summary>
        /// key值
        /// </summary>
        public string ThrottleKey { get; set; }
        /// <summary>
        /// 锁定状态下的key值
        /// </summary>
        public string LockKey { get; set; }
        /// <summary>
        /// 限制策略
        /// </summary>
        public Limiter Limiter { get; set; }
        /// <summary>
        /// 是否限制
        /// </summary>
        public bool IsThrottled { get; set; }
        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLocked { get; set; }
    }
}
