using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QWindowToHttp.HttpService
{
    public class HttpHtmlPageTextValueFormat
    {
        private String html = "";
        public IPEndPoint IPEndPoint { get; set; }
        public Size ComputerScreenSize { get; set; }
        public HttpHtmlPageTextValueFormat(String SourceHtml)
        {
            this.html = SourceHtml;
        }
        public String GetSourceHtml() => html;
        public String GetFormatHtml()
        {
            return html.Replace("{{IPENDPOINT}}", IPEndPoint.Address + ":" + IPEndPoint.Port).Replace("COMPUTERSIZE_W", ComputerScreenSize.Width.ToString()).Replace("COMPUTERSIZE_H", ComputerScreenSize.Height.ToString());
        }
    }
}
