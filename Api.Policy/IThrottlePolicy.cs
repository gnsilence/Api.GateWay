using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Policy
{
  public  interface IThrottlePolicy
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 前缀
        /// </summary>
        string[] Prefixes { get; set; }
        /// <summary>
        /// 策略集合
        /// </summary>
        ICollection<Limiter> Limiters { get; set; }
        /// <summary>
        /// 检测限流
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        CheckResult Check(IThrottleKey key, bool increment = true);
        /// <summary>
        /// 是否被限制
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        bool IsThrottled(IThrottleKey key, out CheckResult result, bool increment = true);
        /// <summary>
        /// 是否被锁定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        bool IsLocked(IThrottleKey key, out CheckResult result, bool increment = true);
    }
}
