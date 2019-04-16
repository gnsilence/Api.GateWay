using System;
using System.Collections.Generic;
using System.Text;
using BeetleX.Buffers;
using BeetleX.FastHttpApi;

namespace Api.GateWay
{
    /// <summary>
    /// 外部重写返回错误
    /// </summary>
   public class BadMessage: InnerErrorResult
    {
        public BadMessage(string errormsg) :base("502", "BadMessage", new Exception(errormsg), false)
        {

        }
        public override string ContentType => "text/html; charset=utf-8";

        public BadMessage(Exception error) : base("502", "BadMessage", error, false)
        {

        }
        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.WriteLine("<html>");
            stream.WriteLine("<body>");
            stream.Write("<h1>");
            stream.WriteLine(Message);
            stream.Write("</h1>");
            if (!string.IsNullOrEmpty(Error))
            {
                stream.Write("<p>");
                stream.WriteLine(Error);
                stream.Write("</p>");
            }
            if (!string.IsNullOrEmpty(SourceCode))
            {
                stream.Write("<p>");
                stream.WriteLine(SourceCode);
                stream.Write("</p>");
            }

            stream.WriteLine("  <hr style=\"margin: 0px; \" /> <p>详细使用查看(<a href=\"https://github.com/ikende\" target=\"_blank\">GitHub</a>)</p>");

            stream.WriteLine("<body>");

            stream.WriteLine("</html>");

        }
    }
}
