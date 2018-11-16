using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DTU_PLC_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbFunc.SelectedIndex = 0;
            cmbData.SelectedIndex = 0;
            btnStop.Enabled = false;
            btnMbSend.Enabled = false;
            btnSend.Enabled = false;
            btnSendLoop.Enabled = false;
        }

        private void ShowMsg(string s)
        {
            if (txtMsg.InvokeRequired)
            {
                txtMsg.Invoke(new MethodInvoker(() =>
                {
                    txtMsg.AppendText(s);
                    txtMsg.AppendText("\r\n");
                    txtMsg.ScrollToCaret();
                }));
            }
            else
            {
                txtMsg.AppendText(s);
                txtMsg.AppendText("\r\n");
                txtMsg.ScrollToCaret();
            }
        }

        private static Thread _listenThread;
        private static Socket _sSocket, _serverSocket;
        private readonly ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        /// <summary>
        /// 服务端Accept回调
        /// </summary>
        /// <param name="iar"></param>
        private void AcceptCallback(IAsyncResult iar)
        {
            try
            {
                var socket = iar.AsyncState as Socket;
                _serverSocket = socket?.EndAccept(iar);
                TimeoutObject.Set();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnStop.Enabled = true;
                btnMbSend.Enabled = true;
                btnSend.Enabled = true;
                btnSendLoop.Enabled = true;
                btnConnect.Enabled = false;
                var ip = IPAddress.Parse(txtIp.Text.Trim());
                var ipe = new IPEndPoint(ip, int.Parse(txtPort.Text.Trim()));
                _sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sSocket.Bind(ipe);
                _sSocket.Listen(0);
                ShowMsg("监听已经打开，请等待...");

                TimeoutObject.Reset();
                //同步Accept改异步，以便设置超时
                IAsyncResult connResult = _sSocket.BeginAccept(new AsyncCallback(AcceptCallback), _sSocket);
                TimeoutObject.WaitOne(20 * 1000, false);

                if (!connResult.IsCompleted)
                {
                    _sSocket.Close();
                    //处理连接不成功的动作
                    ShowMsg("连接超时请重试");
                    btnStop.Enabled = false;
                    btnMbSend.Enabled = false;
                    btnSend.Enabled = false;
                    btnSendLoop.Enabled = false;
                    btnConnect.Enabled = true;
                }
                else
                {
                    ShowMsg("连接已经建立");
                    //创建一个通信线程
                    var pts = new ParameterizedThreadStart(ListenSlave);
                    _listenThread = new Thread(pts) { IsBackground = true };
                    //启动线程
                    _listenThread.Start(_serverSocket);
                    ShowMsg("开始监听从站");
                }

                //原同步方法
                //_serverSocket = _sSocket.Accept();
                //ShowMsg("连接已经建立");
                //创建一个通信线程
                //var pts = new ParameterizedThreadStart(ListenSlave);
                //_listenThread = new Thread(pts) { IsBackground = true };
                ////启动线程
                //_listenThread.Start(_serverSocket);
                //ShowMsg("开始监听从站");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ListenSlave(object s)
        {
            try
            {
                byte[] recByte = new byte[1024];
                while (true)
                {
                    var bytes = _serverSocket.Receive(recByte, recByte.Length, 0);
                    var rcvbt1 = new byte[bytes];
                    Array.Copy(recByte, 0, rcvbt1, 0, bytes);
                    var rst = ModBus.HexByteToHexStr(rcvbt1);
                    ShowMsg(!rst.StartsWith("33") ? "收到消息：" + Encoding.UTF8.GetString(rcvbt1) : "收到消息：" + rst);//33开头的属于modbus码
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static string WriteRead(string hexdata, Socket serverSocket)
        {
            byte[] recByte = new byte[1024];
            var sd = ModBus.HexStrToHexByte(hexdata);//开
            serverSocket.Send(sd, sd.Length, 0);
            int bytes = serverSocket.Receive(recByte, recByte.Length, 0);
            var rcvbt1 = new byte[bytes];
            Array.Copy(recByte, 0, rcvbt1, 0, bytes);
            var rst = ModBus.HexByteToHexStr(rcvbt1);
            var s = rst.Replace("7777772E7573722E636E", "");
            if (s == "")//排除第一个空
            {
                bytes = serverSocket.Receive(recByte, recByte.Length, 0);
                rcvbt1 = new byte[bytes];
                Array.Copy(recByte, 0, rcvbt1, 0, bytes);
                rst = ModBus.HexByteToHexStr(rcvbt1);
                s = rst.Replace("7777772E7573722E636E", "");
            }
            return s;
        }

        private void btnMbSend_Click(object sender, EventArgs e)
        {
            var sd = ModBus.HexStrToHexByte(txtMbData.Text.Trim());//开
            _serverSocket.Send(sd, sd.Length, 0);
            ShowMsg("已发送："+ txtMbData.Text.Trim());
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (cmbFunc.SelectedIndex==1)//读
            {
                var sd = ModBus.ModBusWrite(int.Parse(txtSlave.Text.Trim()), ModBus.ModBusFunction.WriteCoil,
                    int.Parse(txtAddr.Text.Trim()), cmbData.SelectedIndex==0);//写数据
                _serverSocket.Send(sd, sd.Length, 0);
                ShowMsg("已发送：" + ModBus.HexByteToHexStr(sd));
            }
            else//写
            {
                var sd = ModBus.ModBusRead(int.Parse(txtSlave.Text.Trim()), ModBus.ModBusFunction.ReadCoils,
                    int.Parse(txtAddr.Text.Trim()), 1);//写数据
                _serverSocket.Send(sd, sd.Length, 0);
                ShowMsg("已发送：" + ModBus.HexByteToHexStr(sd));
            }
        }

        private Timer timer;
        private void btnSendLoop_Click(object sender, EventArgs e)
        {
            timer = new Timer {Interval = int.Parse(txtLoop.Text.Trim()) * 1000, Enabled = true};
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            btnSend.PerformClick();
        }

        private void btnEndLoop_Click(object sender, EventArgs e)
        {
            timer?.Stop();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer?.Stop();
            _listenThread?.Abort();
            _sSocket?.Close();
            _serverSocket?.Close();
            btnStop.Enabled = false;
            btnMbSend.Enabled = false;
            btnSend.Enabled = false;
            btnSendLoop.Enabled = false;
            btnConnect.Enabled = true;
        }
    }
}
