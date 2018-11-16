using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace XXSD_DamGateControl
{
    public class RunningClass
    {
        private static string _configFile = AppDomain.CurrentDomain.BaseDirectory + "./Config.xml";
        private AsyncHttpServer _webServer = null;
        public static Configuration Config;
        public event Action<string> UpdateMessage;
        public static LogHelper Logger = LogHelper.GetInstance();
        private Kernel _kernel;

        public RunningClass(string configFile = "")
        {
            if (configFile != "")
                _configFile = configFile;
        }

        public bool Start()
        {
            ReadConfig();
            _webServer = new AsyncHttpServer(Config.PublishHost, int.Parse(Config.PublishPort));
            _webServer.OnGetData += WebServer_OnGetData;
            _webServer.Start();
            UpdateMessage?.Invoke("http://" + Config.PublishHost + ":" + Config.PublishPort + " 网络服务启动。");
            _kernel=new Kernel(Config);
            //_kernel.Init();//todo：：：此处初始化服务
            return true;
        }
        public bool Stop()
        {
            //_kernel.Release();//todo：：：此处释放服务
            _webServer.Stop();
            UpdateMessage?.Invoke("http://" + Config.PublishHost + ":" + Config.PublishPort + " 人为停止！");
            return true;
        }

        void WebServer_OnGetData(byte[] bytes, Stream stream)
        {
            try
            {
                var debugon = Config.DebugSwitch.ToLower() == "1" || Config.DebugSwitch.ToLower() == "true";//调试开关，add by：钱春
                var str = System.Text.Encoding.UTF8.GetString(bytes);
                if (debugon)
                {
                    Logger.WriteLog(new LogItem(LogType.Information, "接收到请求："+str.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")));
                }
                var start = str.IndexOf("{");
                var end = str.LastIndexOf("}");
                if (start < 0 || end < 0)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "传入文字,非格式化的Json数据:" + str.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")));
                    return;
                }
                str = str.Substring(start, end - start + 1);

                //todo：：此处执行调用并返回
                Cmd t = WebApiHelper.ParseObjectFromStringInternal<Cmd>(str);
                var json = _kernel.InvokeCmd(t);

                SendInternalData(json, stream);
                if (debugon)
                {
                    Logger.WriteLog(new LogItem(LogType.Information, "返回：" + json));
                }
            }
            catch (Exception e)
            {
                Logger.WriteLog(new LogItem(LogType.Error, e.Message));
            }
        }
        void SendInternalData(string str, Stream stream)
        {
            var content = str;
            //var data = "HTTP/1.1 200 OK\r\n";
            //data += "Content-Length: " + content.Length + "\r\n";
            //data += "Content-Type: text/plain; charset=utf-8\r\n";
            //data += "Server: Microsoft-HTTPAPI/2.0\r\n";
            //data += "Date:" + DateTime.Now.ToString() + "\r\n\r\n";
            var data = content;//data.ToUpper();

            byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);

            // Send back a response.
            stream.Write(msg, 0, msg.Length);
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <returns></returns>
        public static bool ReadConfig()
        {
            XmlSerializer xmldes = new XmlSerializer(typeof(Configuration));
            try
            {
                var file = new XmlTextReader(_configFile);
                Config = xmldes.Deserialize(file) as Configuration;
                file.Close();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(new LogItem(LogType.Error, "读取配置文件[" + _configFile + "]出错，原因：" + ex.Message + "，系统中止。"));
                Environment.Exit(1);
            }
            return true;
        }
    }
}
