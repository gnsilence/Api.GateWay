using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Policy
{
  public  interface IThrottleKey
    {
        object[] Values { get; }
    }
}
