using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Api.Policy.HttpRequest
{
  public  interface IHttpThrottlePolicy
    {
        bool Check(HttpRequestMessageInfo request, out CheckResult result, bool increment = true);

        CheckResult Check(HttpRequestMessageInfo request, bool increment = true);
    }
}
