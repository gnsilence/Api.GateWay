using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Policy.HttpRequest
{
  public  class HttpRequestMessageInfo
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// 方法
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 请求的接口地址不包括ip地址端口
        /// </summary>
        public string RequestUrL { get; set; }
    }
}
