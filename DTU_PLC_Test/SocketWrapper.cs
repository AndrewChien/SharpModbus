using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DTU_PLC_Test
{
    public class SocketClient
    {
        private static int len_buf;
        byte[] buffer;
        Socket commusocket = null;
        string _ip = "";
        int _port = 4531;
        bool _connect = false;
        ManualResetEvent Timeout = new ManualResetEvent(false);
        public SocketClient(string ip, int port) //构造函数设定服务器的ip地址和端口
        {
            _ip = ip;
            _port = port;
            len_buf = 1024;
            buffer = new byte[len_buf];


        }
        public bool Socket_Create_Connect()
        {
            commusocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint Serv = new IPEndPoint(IPAddress.Parse(_ip), _port);
            Timeout.Reset();
            try
            {
                commusocket.BeginConnect(Serv, new AsyncCallback(ConnectCallback), commusocket);//异步connect
            }
            catch (Exception exp)
            {
                StreamWriter mylog = new StreamWriter("log.txt", true); //记录日志文件
                mylog.WriteLine(exp.Message + DateTime.Now.ToLocalTime().ToString());
                mylog.Close();
                return false;
            }
            if (Timeout.WaitOne(10000, false)) //这里等待10s或者是异步的回调函数调用Timeout.set()
            {
                if (_connect)
                    return true;
                else
                    return false;
            }
            else
                return false; //超时
        }
        public bool IsConnect
        {
            get { return _connect; }
            set { _connect = value; }
        }
        public string SyncReceive()//同步接受
        {
            try
            {
                string result = "";
                if (commusocket == null)
                    Socket_Create_Connect();
                else if (!commusocket.Connected) //只能判断上次的连接状况
                {
                    if (!IsConnected())
                    {
                        Reconnect();
                    }
                }
                int length = commusocket.Receive(buffer, buffer.Length, 0);
                if (length > 0)
                {
                    int len = BitConverter.ToInt32(buffer, 0); //这里是自己定义的格式前四位表示信息长度并不包括本身这四位
                    result = Encoding.Unicode.GetString(buffer, 4, len);
                    return result;
                }
                return result;
            }
            catch (Exception exp)
            {
                StreamWriter mylog = new StreamWriter("log.txt", true); //记录日志文件
                mylog.WriteLine(exp.Message + DateTime.Now.ToLocalTime().ToString());
                mylog.Close();
                return "";
            }
        }
        public bool SyncSend(byte[] buf) //同步send
        {
            try
            {
                bool result = false;
                if (commusocket == null)
                    Socket_Create_Connect();
                if (_connect)
                {
                    int len = commusocket.Send(buf);
                    if (len < 1)
                        result = false;
                    result = true;
                }
                return result;
            }
            catch (Exception exp)
            {
                StreamWriter mylog = new StreamWriter("log.txt", true); //记录日志文件
                mylog.WriteLine(exp.Message + DateTime.Now.ToLocalTime().ToString());
                mylog.Close();
                return false;
            }
        }
        public void Disconnect()
        {
            commusocket.Shutdown(SocketShutdown.Both);
            commusocket.Disconnect(true);
            _connect = false;
            commusocket.Close();
            commusocket = null;
        }
        public void Run() //这里的run可以用来循环和服务器通讯用
        {
            IAsyncResult ar = commusocket.BeginReceive(buffer, 0, len_buf, SocketFlags.None, new AsyncCallback(CallReceive), this);
        }
        //每次服务器发送消息过来会调用beginreceive然后会调用回调函数callreceive并在这个函数里
        //调用run函数就会一直监听服务器发送过来的消息
        private void CallReceive(IAsyncResult ar)
        {
            try
            {
                // Socket client = (Socket)ar.AsyncState;
                int length = commusocket.EndReceive(ar);
                if (length > 0)
                {
                    Receive(buffer, length);

                }
                else
                {
                    if (commusocket.Connected)
                    {
                        Reconnect();
                    }
                }
            }
            catch (Exception exp)
            {
                StreamWriter mylog = new StreamWriter("log.txt", true); //记录日志文件
                mylog.WriteLine(exp.Message + DateTime.Now.ToLocalTime().ToString());
                mylog.Close();
                Reconnect();
                while (!_connect)
                {
                    Reconnect();
                    Thread.Sleep(5000); //隔5秒再试
                }
                Run();
            }
            while (!_connect)
            {
                Reconnect();
                Thread.Sleep(5000); //隔5秒再试
            }
            Run();
        }
        private void Receive(byte[] buf, int len)
        {
            MyEventArgs e = new MyEventArgs();
            e.CommandLength = len;
            Array.Copy(buf, 0, e.CommandBuf, 0, len);
            OnCommandArrival(e);
        }
        private bool Reconnect()
        {
            commusocket.Shutdown(SocketShutdown.Both);
            commusocket.Disconnect(true);
            _connect = false;
            commusocket.Close();
            return Socket_Create_Connect();
        }
        private void ConnectCallback(IAsyncResult ar) //connect的异步回调函数
        {
            Socket client = (Socket)ar.AsyncState;
            try
            {
                client.EndConnect(ar);
                _connect = true;
            }
            catch (Exception exp)
            {
                StreamWriter mylog = new StreamWriter("log.txt", true); //记录日志文件
                mylog.WriteLine(exp.Message + DateTime.Now.ToLocalTime().ToString());
                mylog.Close();
                _connect = false;
            }
            finally
            {
                Timeout.Set(); //恢复信号
            }
        }
        private bool IsConnected()//判断是否断线
        {
            bool ConnectState = true;
            bool state = commusocket.Blocking;
            try
            {
                byte[] temp = new byte[1];
                commusocket.Blocking = false;
                commusocket.Send(temp, 0, 0);
                ConnectState = true;
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10035)) //仍然是connect的
                    ConnectState = true;
                else
                    ConnectState = false;
            }
            finally
            {
                commusocket.Blocking = state;
            }
            _connect = ConnectState;
            return ConnectState;
        }
        public class MyEventArgs : EventArgs //类作为委托的一个参数
        {
            private int _command_length;
            private byte[] _command_buf;
            public MyEventArgs()
            {
                this._command_length = 0;
                this._command_buf = new byte[len_buf];
            }
            public int CommandLength
            {
                get { return _command_length; }
                set { _command_length = value; }
            }
            public byte[] CommandBuf
            {
                get { return _command_buf; }
                set { _command_buf = value; }
            }
        }
        public delegate void CommandArrivalEventHandler(object sender, MyEventArgs args);//定义一个委托
        public event CommandArrivalEventHandler CommandArrival;//用委托初始化事件
        protected virtual void OnCommandArrival(MyEventArgs e)//收到服务器的命令做相应的动作
        {
            if (CommandArrival != null)
            {
                CommandArrival(this, e);
            }
        }
    }
}
