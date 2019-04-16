using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Api.Policy.HttpRequest
{
    public class HttpRequestKey : IThrottleKey
    {
        /// <summary>
        /// 这里可以根据需要拓展获取值的方式，比如直接从request获取
        /// </summary>
        public string IPAddress { get; set; }
        public string Method { get; set; }
        public string RequestUrL { get; set; }
        public virtual object[] Values
        {
            get
            {
                return new object[]
                {
                    IPAddress,
                    Method,
                    RequestUrL
                };
            }
        }

        public virtual void Initialize(HttpRequestMessageInfo request)
        {
            IPAddress = request.IPAddress;
            Method = request.Method;
            RequestUrL = request.RequestUrL;
        }
        //private IPAddress GetClientIp()
        //{
        //    if (Request.Headers.Contains("X-Real-IP"))
        //    {
        //        var reslut = Request.Headers.GetValues("X-Real-IP");
        //        foreach (var item in reslut)
        //        {
        //            return IPAddress.Parse(item);
        //        }
        //    }

        //    if (Request.Properties.ContainsKey("IPAddress"))
        //    {
        //        return
        //            IPAddress.Parse(
        //                (Request.Properties["IPAddress"]).ToString());
        //    }

        //    throw new InvalidOperationException("IPAddress not found");
        //}
    }
}
