using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Api.GateWay.ConfigSetting
{
    internal class OwnLifetime : IHostLifetime
    {
        private Logger logger = new LogFactory().GetCurrentClassLogger();
        private IApplicationLifetime ApplicationLifetime { get; }

        public OwnLifetime(IApplicationLifetime applicationLifetime, IServiceProvider services)
        {
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Info("服务停止");
            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            logger.Info("服务开始启动。。");
            return Task.CompletedTask;
        }
    }
}
