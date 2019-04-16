using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Policy
{
    public class GenericKey : IThrottleKey
    {
        /// <summary>
        /// 普通key
        /// </summary>
        /// <param name="values"></param>
        public GenericKey(params object[] values)
        {
            Values = values;
        }
        public object[] Values { get; private set; }
    }
}
