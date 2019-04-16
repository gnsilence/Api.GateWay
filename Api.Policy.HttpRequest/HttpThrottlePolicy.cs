using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Api.Policy.HttpRequest
{
   public class HttpThrottlePolicy<T> : HttpThrottlePolicyBase where T : HttpRequestKey, new()
    {
        public HttpThrottlePolicy(IThrottleRepository repository)
           : base(repository)
        {
        }
        public virtual T CreateIdentity(HttpRequestMessageInfo request)
        {
            var identity = new T();
            identity.Initialize(request);
            return identity;
        }

        public override bool Check(HttpRequestMessageInfo request, out CheckResult result, bool increment = true)
        {
            result = Check(request, increment);
            return result.IsThrottled;
        }

        public override CheckResult Check(HttpRequestMessageInfo request, bool increment = true)
        {
            T identity = CreateIdentity(request);
            return base.Check(identity, increment);
        }
    }
}
