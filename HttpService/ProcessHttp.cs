using QWindowToHttp.WindowTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace QWindowToHttp.HttpService
{
    public class ProcessHttp
    {
        public HttpListener listen = new HttpListener();

        public static event Action<String> Message;
        public static event Action<string> Error;
        public readonly IEnumerable<IPEndPoint> UsingIpEndPoints = null;
        private HttpHtmlPageTextValueFormat MainPageHtmlText = null;
        public bool Life { get; set; } = true;
        public ProcessHttp(int Port = 9003)
        {
            OnMessage("Http初始化 开始");
            List<IPEndPoint> ips = new List<IPEndPoint>();
            foreach (var i in GetIpv4IpAddresss())
            {
                listen.Prefixes.Add("http://" + i.ToString() + ":" + Port + "/");
                var ip = new IPEndPoint(i, Port);
                ips.Add(ip);
                OnMessage("添加监听：" + ip.ToString());
            }
            UsingIpEndPoints = ips;
            ReloadHtmlFile();
            OnMessage("Http初始化 完毕");
            Start();

        }
        public void ReloadHtmlFile()
        {
            MainPageHtmlText = new HttpHtmlPageTextValueFormat(File.ReadAllText("./MainPage.Html"))
            {
                ComputerScreenSize = Computer.ScreenSize,
                IPEndPoint = UsingIpEndPoints.Last()
            };
            OnMessage("刷新主页面数据");
        }
        private void Start()
        {
            listen.Start();
            OnMessage("Http开始监听上下文");
            var runTask = Task.Run(() =>
            {
                while (Life)
                {
                    try
                    {
                        var context = listen.GetContext();
                        OnReceive(context);
                    }
                    catch (Exception error)
                    {
                        OnError("Http请求上下文异常\n" + error.Message);
                    }
                }
                OnMessage("Http进程关闭");
            });
            OnMessage("开启了监听进程" + runTask.Id);
        }

        private void OnReceive(HttpListenerContext context)
        {
            var response = context.Response;
            var request = context.Request;

            response.Headers.Add("Access-Control-Allow-Origin", "*"); // 允许所有源访问，或者指定一个具体的源，如 "http://example.com"  
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"); // 允许的HTTP方法  
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept"); // 允许的HTTP头  

            Task.Run(() =>
            {
                try
                {
                    switch (context.Request.Url.LocalPath)
                    {
                        case "/keys":
                            {
                                ResponseJson(Computer.AccessKeys.ToJsArrayString());
                            }
                            break;
                        case "/desktop":
                            {
                                var img = GdiTool.GetScreenImageMemoryJpeg();
                                ResponseImage(img);
                            }; break;
                        case "/control":
                            {
                                foreach (var controlKey in request.QueryString)
                                {
                                    var key = controlKey.ToString().ToLower();
                                    var value = request.QueryString.Get(key);

                                    switch (key)
                                    {
                                        case "mouse_move":
                                            {
                                                var data = value.Split(',');
                                                Computer.MousePoint = new System.Drawing.Point(int.Parse(data[0]), int.Parse(data[1]));
                                                ResponseBoolean(true);
                                            }; break;
                                        case "mouse_down":
                                            {
                                                var data = value.Split(',');
                                                Computer.MouseDown(new System.Drawing.Point(int.Parse(data[0]), int.Parse(data[1])));
                                                ResponseBoolean(true);
                                            }; break;
                                        case "mouse_up":
                                            {
                                                var data = value.Split(',');
                                                Computer.MouseUp(new System.Drawing.Point(int.Parse(data[0]), int.Parse(data[1])));
                                                ResponseBoolean(true);
                                            }; break;
                                        case "mouse_click":
                                            {
                                                var data = value.Split(',');
                                                if (data.Length > 1)
                                                {
                                                    Computer.MouseClick(new System.Drawing.Point(int.Parse(data[0]), int.Parse(data[1])));
                                                }
                                                else
                                                {
                                                    Computer.MouseClick();
                                                }
                                                ResponseBoolean(true);
                                            }; break;
                                        case "mouser_click":
                                            {
                                                var data = value.Split(',');
                                                if (data.Length > 1)
                                                {
                                                    Computer.MouseApi.MouseRightDown(int.Parse(data[0]), int.Parse(data[1]));
                                                    Computer.MouseApi.MouseRightUp(int.Parse(data[0]), int.Parse(data[1]));
                                                }
                                                else
                                                {
                                                    var pos = Computer.MousePoint;
                                                    Computer.MouseApi.MouseRightDown(pos.X,pos.Y);
                                                    Computer.MouseApi.MouseRightUp(pos.X,pos.Y);
                                                }
                                                ResponseBoolean(true);
                                            }; break;
                                        case "key_down":
                                            {
                                                var data = int.Parse(value);
                                                Computer.KeyDown(data);

                                            }; break;
                                        case "key_up":
                                            {
                                                var data = int.Parse(value);
                                                Computer.KeyUp(data);
                                            }; break;
                                    }
                                }
                            }; break;
                        default:
                            {
                                ResponseHtml(this.MainPageHtmlText.GetFormatHtml());
                                //ResponseHtml("<h1>默认页</h1>");
                            }
                            break; ;
                    }
                }
                catch (Exception error)
                {
                    ResponseHtml("输入有误" + error);
                    OnError(error.Message);
                }
                context.Response.Close();
            });




            OnMessage("用户请求：" + context.Request.RemoteEndPoint + "\t" + context.Request.Url.ToString());




            void ResponseImage(Stream ImageStream, bool IsDownload = false, String Extention = "jpeg")
            {
                ImageStream.Position = 0;
                ImageStream.CopyTo(response.OutputStream);
                response.Headers.Set(HttpResponseHeader.ContentType, "image/" + Extention);
                response.Headers.Set("Content-Disposition", string.Format("{0}; filename=\"{1}\"", IsDownload ? "attachment" : "inline", "deskto_image." + Extention));
            }
            void ResponseHtml(string HtmlText)
            {
                response.Headers.Set(HttpResponseHeader.ContentType, "text/html;charset=utf-8");
                response.OutputStream.Write(Encoding.UTF8.GetBytes(HtmlText));
            }
            void ResponseJson(string Json)
            {
                response.Headers.Set(HttpResponseHeader.ContentType, "application/json;charset=utf-8");
                response.OutputStream.Write(Encoding.UTF8.GetBytes(Json));
            }
            void ResponseBoolean(Boolean b)
            {
                response.Headers.Set(HttpResponseHeader.ContentType, "text/plain;charset=utf-8");
                response.OutputStream.Write(Encoding.UTF8.GetBytes(b.ToString()));
            }


        }


        private void OnError(string msg)
        {
            Error?.Invoke(msg);
        }
        private void OnMessage(string msg)
        {
            Message?.Invoke(msg);
        }
        private static IPAddress[] GetIpv4IpAddresss()
        {
            List<IPAddress> ips = new List<IPAddress>();
            ips.Add(IPAddress.Parse("127.0.0.1"));
            var inter = NetworkInterface.GetAllNetworkInterfaces().Where(w => w.OperationalStatus == OperationalStatus.Up && w.NetworkInterfaceType != NetworkInterfaceType.Loopback);
            foreach (var i in inter)
            {
                ips.AddRange(i.GetIPProperties().UnicastAddresses.Where(w => w.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(w => w.Address));
            }
            return ips.ToArray();
        }
    }
}
