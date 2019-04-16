using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Api.GateWay.ConfigSetting
{
    public class ConfigSettings
    {
        /// <summary>
        /// 设置只读配置
        /// </summary>
        private static readonly Lazy<ConfigSettings> _instance = new Lazy<ConfigSettings>(() => new ConfigSettings());

        public static ConfigSettings Instance => _instance.Value;

        public IConfiguration Configuration { get; }

        private ConfigSettings()
        {
            //获取项目下config.json的配置信息

            var path = AppDomain.CurrentDomain.BaseDirectory;
            Configuration = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("ApiConfig.json", true, true)
                .Add(new JsonConfigurationSource { Path = "ApiConfig.json", Optional = true, ReloadOnChange = true })
                .Build();

            //绑定服务到集合
            Configuration.GetSection("Config:ServerList").Bind(HostServers);
        }
        /// <summary>
        /// 接口端口
        /// </summary>
        public int ApiPort => Convert.ToInt32(Configuration["Config:ApiServerPort"]);

        /// <summary>
        /// 网关端口
        /// </summary>
        public int GateWayPort => Convert.ToInt32(Configuration["Config:GateWayPort"]);

        /// <summary>
        /// 服务地址
        /// </summary>
        public List<HostServer> HostServers { get; } = new List<HostServer>();
        /// <summary>
        /// 限流时间
        /// </summary>
        public int LimitTime => Convert.ToInt32(Configuration["Config:LimitTime"]);
        /// <summary>
        /// 限流次数
        /// </summary>
        public int LimitCount => Convert.ToInt32(Configuration["Config:LimitCount"]);
        /// <summary>
        /// 是否开启限流
        /// </summary>
        public bool IsLimit => Convert.ToBoolean(Configuration["Config:IsLimit"]);
        /// <summary>
        /// redis地址
        /// </summary>
        public string RedisServerConnectionString => Configuration["Config:RedisServer"];

    }

    /// <summary>
    /// 服务地址
    /// </summary>
    public class HostServer
    {
        /// <summary>
        /// URi 地址和端口
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 参与负载的权重(0-10,0代表不参与负载，即所有服务器不可用才使用)
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// URL请求匹配规则
        /// </summary>
        public string UrlPrex { get; set; }
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnections { get; set; }
        /// <summary>
        /// 每秒最大请求数
        /// </summary>
        public int MaxRps { get; set; }
        /// <summary>
        /// 一致性负载，参数：[host|url|baseurl|(h:name)|(q:name)]
        /// </summary>
        /*
         * 可以根据实际情况选择其中一种方式
         * Host 使用Header的Host作为一致性转发
         * url 使用整个Url作为一致性转发
         * baseurl 使用整个BaseUrl作为一致性转发
         * h:name 使用某个Header值作为一致性转发
         * q:name 使用某个QueryString值作为一致性转发
         *
         */
        public string HashPattern { get; set; }
    }
}
