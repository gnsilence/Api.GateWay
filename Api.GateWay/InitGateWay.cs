using Api.GateWay.ConfigSetting;
using Api.GateWay.Filter;
using Api.Policy;
using Api.Policy.HttpRequest;
using Api.Policy.Redis;
using BeetleX.FastHttpApi;
using Bumblebee;
using Bumblebee.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Api.GateWay
{
    public class InitGateWay
    {


        #region 定义网关网关
        //Nlog
        private static Logger logger = new LogFactory().GetCurrentClassLogger();
        //定义网关
        private static Gateway g;
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns></returns>
        public Task SetServer()
        {
            g = new Gateway();
            g.HttpOptions(h =>
            {
                h.Port = ConfigSettings.Instance.GateWayPort;//端口
                h.LogToConsole = true;//开启控制台日志显示
                h.LogLevel = BeetleX.EventArgs.LogType.Warring;//日志等级
                h.WriteLog = true;//是否写日志
                h.UseIPv6 = true;//使用ipv6
                h.SessionTimeOut = 5000;//设置超时时间  
            });
            ConfigSettings.Instance.HostServers.ForEach(k =>
            {
                g.SetServer(k.Host, k.MaxConnections).AddUrl(k.UrlPrex, k.HashPattern, k.Weight, k.MaxRps);
            });
            g.LoadPlugin(typeof(RequestFilter).Assembly);
            g.Routes.Default.Pluginer.SetRequesting("Requesting");
            g.Routes.Default.Pluginer.SetAgentRequesting("Agent");
            g.Routes.Default.Pluginer.SetRequested("Requested");
            g.Routes.Default.Pluginer.SetHeaderWriting("Headerwrite");
            g.Pluginer.SetResponseError("responseError");
            g.AgentMaxConnection = 10000;//代理的最大连接数
            g.AgentMaxSocketError = 10000;//最大错误数

            /*
             * (支持两种动态添加url的方式)
             * 1，监视配置文件变化,发生改变时更新服务列表
             * 2，页面添加，访问http://iP+端口+/__admin/
             */
            IFileProvider fileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory);
            ChangeToken.OnChange(() => fileProvider.Watch("ApiConfig.json"), () => LoadAppSeetings());
            g.Open();
            g.LoadPlugin(typeof(Bumblebee.Configuration.Management).Assembly);
            return Task.CompletedTask;
        }
        #endregion


        #region 动态加载配置文件
        /// <summary>
        /// 动态监视配置文件,发生改变时重新加载添加的url
        /// </summary>
        private void LoadAppSeetings()
        {
            logger.Info("配置文件发生变化,重新添加服务列表");
            var HostServers = new List<HostServer>();

            //获取项目下config.json的配置信息

            var path = AppDomain.CurrentDomain.BaseDirectory;
            var Configuration = new ConfigurationBuilder()
                  .SetBasePath(path)
                  .AddJsonFile("ApiConfig.json", true, true)
                  .Add(new JsonConfigurationSource { Path = "ApiConfig.json", Optional = true, ReloadOnChange = true })
                  .Build();

            //绑定服务到集合
            Configuration.GetSection("Config:ServerList").Bind(HostServers);
            HostServers.ForEach(k =>
            {
                var urllist = new List<string>();
                foreach (var server in g.Agents.Servers)
                {
                    urllist.Add(server.Uri.AbsoluteUri.Trim('/'));
                }
                //不存在地址则添加
                if (!urllist.Exists(p => p == k.Host))
                {
                    g.SetServer(k.Host, k.MaxConnections).AddUrl(k.UrlPrex, k.HashPattern, k.Weight, k.MaxRps);
                    logger.Info($"添加服务地址:{k.Host}");
                }
            });
        }
        #endregion


        #region 停止服务
        /// <summary>
        /// 停止服务
        /// </summary>
        /// <returns></returns>
        public Task StopServer()
        {
            g.Dispose();
            return Task.CompletedTask;
        }
        #endregion

    }
}
