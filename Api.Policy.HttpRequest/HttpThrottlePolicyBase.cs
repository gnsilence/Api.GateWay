using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Api.Policy.HttpRequest
{
    public abstract class HttpThrottlePolicyBase : ThrottlePolicy, IHttpThrottlePolicy
    {
        public HttpThrottlePolicyBase()
            : this(new MemoryThrottleRepository())
        {
        }

        public HttpThrottlePolicyBase(IThrottleRepository throttleRepository)
            : base(throttleRepository)
        {
        }
        public abstract bool Check(HttpRequestMessageInfo request, out CheckResult result, bool increment = true);

        public abstract CheckResult Check(HttpRequestMessageInfo request, bool increment = true);
    }
}
