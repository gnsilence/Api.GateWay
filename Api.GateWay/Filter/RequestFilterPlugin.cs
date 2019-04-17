using System;
using System.Collections.Generic;
using System.Text;
using Bumblebee;
using Bumblebee.Events;
using System.Reflection;
using Api.Policy.Redis;
using Api.GateWay.ConfigSetting;
using Api.Policy.HttpRequest;
using Api.Policy;
using NLog;
using BeetleX.FastHttpApi;
using Bumblebee.Plugins;

namespace Api.GateWay.Filter
{
    /// <summary>
    /// 请求拦截
    /// </summary>
    public class RequestFilter : IRequestingHandler
    {
        //插件名称
        public string Name => "Requesting";
        /// <summary>
        /// 插件描述
        /// </summary>
        public string Description => "filter requesting";

        //Nlog
        private static Logger logger = new LogFactory().GetCurrentClassLogger();
        public void Execute(EventRequestingArgs e)
        {
            if (ConfigSettings.Instance.IsLimit)
            {
                var ipaddress = e.Request.RemoteIPAddress.Substring(0, e.Request.RemoteIPAddress.LastIndexOf(':'));
                HttpRequestMessageInfo httpRequest = new HttpRequestMessageInfo()
                {
                    IPAddress = ipaddress,
                    Method = e.Request.Method,
                    RequestUrL = e.Request.BaseUrl
                };
                //缓存限流
                var reslut =UseCachePolicyWithLocking.Check(new GenericKey(ipaddress));
                //redis 限流
                //var reslut = apiRequestPolicy.Check(httpRequest);

                if (reslut.IsThrottled)
                {
                    e.Cancel = true;
                    e.Gateway.Response(e.Response, new NotFoundResult("请求太频繁，请稍后再试"));
                    Console.WriteLine($"访问太频繁，请稍后再试");
                }
                if (reslut.IsLocked)
                {
                    e.Gateway.Response(e.Response, new NotFoundResult("请求太频繁，请稍后再试"));
                    e.Cancel = true;
                    Console.WriteLine($"访问太频繁，已被锁定{DateTime.Now}锁定时间{reslut.Limiter.LockDuration}");
                }
            }
        }

        public void Init(Gateway gateway, Assembly assembly)
        {

        }

        #region 使用redis限流
        ///// <summary>
        /////使用redis
        ///// </summary>
        //static RedisThrottleRepository repository = new RedisThrottleRepository(ConfigSettings.Instance.RedisServerConnectionString);
        ///// <summary>
        ///// 使用redis限流
        ///// </summary>
        //HttpThrottlePolicy<HttpRequestKey> apiRequestPolicy = new HttpThrottlePolicy<HttpRequestKey>(repository) // Pass in the Redis repository
        //{
        //    Name = "limit  Requests by redis",
        //    Prefixes = new[] { "requests" },
        //    //白名单
        //    IgnoreList = new[] { "127.0.0.1", "::1" },
        //    //PerSecond = 1000, // 每秒多少次
        //    //可以组合限流
        //    Limiters = new Limiter[]
        //   {
        //       //限制不锁定
        //        new Limiter()
        //       .Limit(100)                        // 限制访问数 100 requests,
        //       .Over(TimeSpan.FromSeconds(1))     // 时间,
        //       ,
        //       new Limiter()
        //       .Limit(10000)
        //       .Over(TimeSpan.FromMinutes(5))//5分钟超过会被锁定
        //       .LockFor(TimeSpan.FromMinutes(5))
        //       //一个小时内访问超过被锁定1个小时不得访问
        //       ,new Limiter()
        //       .Limit(100000)
        //       .Over(TimeSpan.FromHours(1))
        //       .LockFor(TimeSpan.FromHours(1))
        //       ,
        //            //一天内访问超过被锁定3天内不得访问接口
        //        new Limiter()
        //       .Limit(300000)
        //       .Over(TimeSpan.FromDays(1))
        //       .LockFor(TimeSpan.FromDays(3))
        //   }
        //};
        #endregion

        #region 使用缓存限流
        /// <summary>
        /// 使用缓存限流
        /// </summary>
        ThrottlePolicy UseCachePolicyWithLocking = new ThrottlePolicy()
        {
            Name = "limit by MemoryCache",
            Prefixes = new[] { "limitwithlocking" },
            IgnoreList = new[] { "127.0.0.1", "::1" },
            Limiters = new Limiter[]
            {
                    new Limiter()
                    .Limit(200)                        // 限制访问数 200 requests,
                    .Over(TimeSpan.FromSeconds(1))     // 限制时间
                    .LockFor(TimeSpan.FromMinutes(1))  // 锁定时间，锁定时间内不允许访问
            }
        };
        #endregion
       
    }

    /// <summary>
    /// 代理拦截
    /// </summary>
    public class AgentRequesting : IAgentRequestingHandler
    {
        public string Name => "Agent";

        public string Description => "filter Agent";

        public void Execute(EventAgentRequestingArgs e)
        {
            Console.WriteLine($"{e.Request.Url}");
        }

        public void Init(Gateway gateway, Assembly assembly)
        {
        }
    }

    /// <summary>
    /// 添加请求头
    /// </summary>
    public class HeaderWriting : IHeaderWritingHandler
    {
        public string Name => "Headerwrite";

        public string Description => "add header to request";

        public void Execute(EventHeaderWritingArgs e)
        {
            e.Header.Add("name","tom");
        }

        public void Init(Gateway gateway, Assembly assembly)
        {
        }
    }

    /// <summary>
    /// 请求完成处理
    /// </summary>
    public class Requested : IRequestedHandler
    {
        private static Logger logger = new LogFactory().GetCurrentClassLogger();
        public string Name => "Requested";

        public string Description => "Requested filter";

        public void Execute(EventRequestCompletedArgs e)
        {
            logger.Info($"请求时长：{e.Time} ms");
            Console.WriteLine($"请求时长：{e.Time} ms");
        }

        public void Init(Gateway gateway, Assembly assembly)
        {
        }
    }

    /// <summary>
    /// 错误处理
    /// </summary>
    public class ResponseErrors : IResponseErrorHandler
    {
        //Nlog
        private static Logger logger = new LogFactory().GetCurrentClassLogger();
        public string Name => "responseError";

        public string Description => "filter ResponseError";

        public void Exeucte(EventResponseErrorArgs e)
        {
            var code = e.ErrorCode;
            logger.Error(e.Result);
            if (code == 591 || code == 590)//转发的接口地址不可访问
            {
                Console.WriteLine($"错误码：{e.ErrorCode}");
                e.Result = new BadMessage("访问出错,所访问的地址不可用");
            }
            e.Result = new BadMessage("访问出错");
        }

        public void Init(Gateway gateway, Assembly assembly)
        {
        }
    }
}
