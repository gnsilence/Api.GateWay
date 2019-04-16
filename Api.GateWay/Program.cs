using BeetleX.FastHttpApi;
using Bumblebee;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Microsoft.Extensions.Logging;
using NLog;
using Bumblebee.Events;
using System.Linq;
using System.Collections.Concurrent;
using Api.GateWay.Filter;
using System.IO;
using System.IO.Pipes;
using Api.Policy.Redis;
using Api.Policy.HttpRequest;
using Api.Policy;
using System.Net.Http;
using Api.GateWay.ConfigSetting;

namespace Api.GateWay
{
    class Program
    {
        #region 创建服务相关.使用Generic host
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    //使用topshelf接管生命周期处理
                    services.AddSingleton<IHostLifetime, OwnLifetime>();
                    services.AddHostedService<HttpServerHosted>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddEventLog();//启用系统事件日志，
                })
                ;
            //使用docker发布的时候注释掉topshelf
            //builder.Build().Run();
            HostFactory.Run(x =>
            {
                //服务名称
                x.SetServiceName("GateWayService");
                //服务展示名称
                x.SetDisplayName("GateWayService");
                //服务描述
                x.SetDescription("网关服务");
                //服务启动类型：自动
                x.StartAutomatically();
                //x.RunAsLocalSystem();
                x.Service<IHost>(s =>
                {
                    s.ConstructUsing(() => builder.Build());

                    s.WhenStarted(service =>
                    {
                        service.Start();
                    });

                    s.WhenStopped(service =>
                    {
                        service.StopAsync();
                    });
                });
            });
        }
        #endregion

        #region 网关相关方法
        public class HttpServerHosted : IHostedService
        {
            InitGateWay initGateWay = new InitGateWay();
            public virtual Task StartAsync(CancellationToken cancellationToken)
            {
                initGateWay.SetServer();
                return Task.CompletedTask;
            }
           

            public virtual Task StopAsync(CancellationToken cancellationToken)
            {
                initGateWay.StopServer();
                return Task.CompletedTask;
            }
         
        }
        #endregion

    }
}
