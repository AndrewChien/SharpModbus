using System;
using System.IO;
using System.Net;
using System.Threading;

namespace XXSD_DamGateControl
{
    class AsyncHttpServer
    {
        private HttpListener Server = null;
        private Thread ListenThread;
        public event Action<string> OnError;
        public event Action<byte[], Stream> OnGetData;
        
        public AsyncHttpServer(string host, int port)
        {
            Server = new HttpListener(); 
            Server.Prefixes.Add("http://" + host + ":" + port + "/");//格式：http://127.0.0.1:8888/
        }
        public void Start()
        {
            ListenThread = new Thread(new ThreadStart(ListenForClients));
            ListenThread.Start();
        }
        public void Stop()
        {
            Server.Stop();
            ListenThread.Abort();
        }
        private void ListenForClients()
        {
            Server.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext request = Server.GetContext();
                    ThreadPool.QueueUserWorkItem(processRequest, request);
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                        OnError("错误，接收http请求时失败，原因：" + ex.Message);
                }
            }
        }
        public void processRequest(object listenerContext)
        {
            try
            {
                var context = (HttpListenerContext)listenerContext;
                var bytes = new byte[context.Request.ContentLength64];
                context.Request.InputStream.Read(bytes, 0, bytes.Length);
                if (OnGetData != null)
                    OnGetData(bytes, context.Response.OutputStream);
                context.Response.Close();
            }
            catch(Exception ex)
            {
                OnError?.Invoke("错误，处理请求失败，原因：" + ex.Message);
            }
        }
    }
}
