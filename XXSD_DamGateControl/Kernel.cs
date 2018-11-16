using System;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Modbus.Data;
using Modbus.Device;

namespace XXSD_DamGateControl
{
    /// <summary>
    /// AndrewChien-lyo 2018-11-11 22:52:28
    /// </summary>
    public class Kernel
    {
        #region 变量区

        private static string comPort = "", baudRate = "", dataBits = "", parity = "", 
            stopBits = "", slaveId="", registerAdd1="", registerAdd2 = "", registerAdd3 = "",Delay="", TcpAddress="", TcpPort="", SocketDelay="";
        static bool DebugSwitchOn = false;
        static LogHelper Logger = LogHelper.GetInstance();

        private static IPEndPoint ipe;
        private static Socket _sSocket, _serverSocket;
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        #endregion

        #region 构造析构区

        public Kernel(Configuration config)
        {
            comPort = config.ComPort;
            baudRate = config.BaudRate;
            dataBits = config.DataBits;
            parity = config.Parity;
            stopBits = config.StopBits;
            slaveId = config.SlaveId;
            Delay = config.Delay;
            registerAdd1 = config.RegisterAdd1;
            registerAdd2 = config.RegisterAdd2;
            registerAdd3 = config.RegisterAdd3;
            DebugSwitchOn = config.DebugSwitch == "1";
            TcpAddress = config.TcpAddress;
            TcpPort = config.TcpPort;
            SocketDelay = config.SocketDelay;

            //改成长连接，启动服务时监听
            new Thread(new ThreadStart(ConnectAcceptSync)).Start();
        }

        /// <summary>
        /// 同步方法（一直保持连接状态）
        /// </summary>
        private static void ConnectAcceptSync()
        {
            try
            {
                ipe = new IPEndPoint(IPAddress.Parse(TcpAddress), int.Parse(TcpPort));
                _sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sSocket.Bind(ipe);
                _sSocket.Listen(20);
                Logger.WriteLog(new LogItem(LogType.Error, "Modbus主站已启动，等待连接完成..."));

                while (true)//服务端一直保持监听状态，防止socket超时
                {
                    _serverSocket = _sSocket.Accept();
                    Logger.WriteLog(new LogItem(LogType.Error, "Modbus主站已连接完成，正在等待调用..."));
                }
            }
            catch (Exception ex)
            {
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "ConnectAccept:" + ex.Message));
                }
            }
        }

        /// <summary>
        /// 异步方法
        /// </summary>
        private static void ConnectAccept()
        {
            try
            {
                ipe = new IPEndPoint(IPAddress.Parse(TcpAddress), int.Parse(TcpPort));
                _sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sSocket.Bind(ipe);
                _sSocket.Listen(0);
                Logger.WriteLog(new LogItem(LogType.Error, "Modbus主站已启动，等待连接完成..."));

                TimeoutObject.Reset();
                //同步Accept改异步，以便设置Accept超时
                IAsyncResult connResult = _sSocket.BeginAccept(new AsyncCallback(AcceptCallback), _sSocket);
                TimeoutObject.WaitOne(int.Parse(SocketDelay) * 1000, false);

                if (!connResult.IsCompleted)
                {
                    _sSocket.Close();
                    //处理连接不成功的动作
                    Logger.WriteLog(new LogItem(LogType.Error, "连接超时，正在重新连接..."));
                    ConnectAccept();//无限循环连接
                }
                else
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "Modbus连接已建立，已监听到从站。等待服务调用信号..."));
                }
            }
            catch (Exception ex)
            {
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "ConnectAccept:" + ex.Message));
                }
            }
        }

        private static void AcceptCallback(IAsyncResult iar)
        {
            try
            {
                var socket = iar.AsyncState as Socket;
                _serverSocket = socket?.EndAccept(iar);
                TimeoutObject.Set();
            }
            catch (Exception ex)
            {
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "AcceptCallback:" + ex.Message));
                }
            }
        }

        ~Kernel()
        {
            try
            {
                _sSocket.Close();
                _sSocket.Dispose();
                _serverSocket.Close();
                _serverSocket.Dispose();
            }
            catch (Exception ex)
            {
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "~Kernel:" + ex.Message));
                }
            }
        }
        
        #endregion

        public string InvokeCmd(Cmd cmd)
        {
            var rtn = "";
            switch (cmd.command)
            {
                case "OriginRead":
                    rtn = OriginReadCom(cmd);//原始方法，仅供调试，不建议使用。需要寄存器地址
                    break;
                case "OriginWrite":
                    rtn = OriginWriteCom(cmd);//原始方法，仅供调试，不建议使用。需要寄存器地址、写入的值（bool量）
                    break;
                case "FstCotCom":
                    rtn = FstCotCom();//第一组信号控制
                    break;
                case "SecCotCom":
                    rtn = SecCotCom();//第二组信号控制
                    break;
                case "ThdCotCom":
                    rtn = ThdCotCom();//第三组信号控制
                    break;
                case "TcpControll":
                    rtn = TcpControll(cmd);//TCP手动控制开闭（写寄存器）、读寄存器
                    break;
                case "TcpAuto":
                    rtn = TcpAuto(cmd);//TCP触发自动控制开闭（自动延时开闭），仅用于写寄存器
                    break;
                default:
                    break;
            }
            return rtn;
        }

        #region TCP通讯区域

        /// <summary>
        /// 自动延时控制开闭（仅写寄存器）
        /// (必须值：cmd.SlaveId，cmd.Address)
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string TcpAuto(Cmd cmd)
        {
            try
            {
                //ipe = new IPEndPoint(IPAddress.Parse(TcpAddress), int.Parse(TcpPort));
                //using (_sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                //{
                //    //_sSocket.ReceiveTimeout = int.Parse(SocketDelay) * 1000;
                //    //sSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, -300);
                //    _sSocket.Bind(ipe);
                //    _sSocket.Listen(0);

                //    TimeoutObject.Reset();
                //    //同步Accept改异步，以便设置超时
                //    IAsyncResult connResult = _sSocket.BeginAccept(new AsyncCallback(AcceptCallback), _sSocket);
                //    TimeoutObject.WaitOne(int.Parse(SocketDelay) * 1000, false);

                //    if (!connResult.IsCompleted)
                //    {
                //        //_serverSocket.Close();
                //        _sSocket.Close();
                //        //处理连接不成功的动作
                //        if (DebugSwitchOn)
                //        {
                //            Logger.WriteLog(new LogItem(LogType.Error, "连接超时"));
                //        }
                //        return "连接超时请重试";
                //    }
                //    else
                //    {
                //        WriteRead(ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                //            int.Parse(cmd.address), true), _serverSocket);//写开状态
                //        Thread.Sleep(string.IsNullOrEmpty(cmd.value) ? int.Parse(Delay) : int.Parse(cmd.value));//cmd.Value替代默认值Delay
                //        WriteRead(ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                //            int.Parse(cmd.address), false), _serverSocket);//写闭状态
                //        _serverSocket.Close();
                //        _sSocket.Close();
                //        return "调用成功";
                //    }
                //}

                WriteRead(ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                    int.Parse(cmd.address), true), _serverSocket);//写开状态
                Thread.Sleep(string.IsNullOrEmpty(cmd.value) ? int.Parse(Delay) : int.Parse(cmd.value));//cmd.Value替代默认值Delay
                WriteRead(ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                    int.Parse(cmd.address), false), _serverSocket);//写闭状态
                return "调用成功";
            }
            catch (Exception e)
            {
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, e.Message));
                }
                return e.Message;
            }
        }

        public static string TcpControll(Cmd cmd)
        {
            try
            {
                //var rtn = "";
                //ipe = new IPEndPoint(IPAddress.Parse(TcpAddress), int.Parse(TcpPort));
                //using (_sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                //{
                //    _sSocket.Bind(ipe);
                //    _sSocket.Listen(0);

                //    TimeoutObject.Reset();
                //    //同步Accept改异步，以便设置超时
                //    IAsyncResult connResult = _sSocket.BeginAccept(new AsyncCallback(AcceptCallback), _sSocket);
                //    TimeoutObject.WaitOne(int.Parse(SocketDelay) * 1000, false);

                //    if (!connResult.IsCompleted)
                //    {
                //        //_serverSocket.Close();
                //        _sSocket.Close();
                //        //处理连接不成功的动作
                //        if (DebugSwitchOn)
                //        {
                //            Logger.WriteLog(new LogItem(LogType.Error, "连接超时"));
                //        }
                //        return "连接超时请重试";
                //    }
                //    else
                //    {
                //        if (cmd.readwrite == "write") //写
                //        {
                //            var data = cmd.value == "1";
                //            var sd = ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                //                int.Parse(cmd.address), data); //写数据
                //            rtn = "调用成功-" + WriteRead(sd, _serverSocket);
                //        }
                //        else //读
                //        {
                //            var sd = ModBus.ModBusRead(int.Parse(cmd.slaveid), ModBus.ModBusFunction.ReadCoils,
                //                int.Parse(cmd.address), 1); //读数据
                //            var rst = WriteRead(sd, _serverSocket);
                //            //解析返回数据
                //            rtn = rst.Length == 14 ? rst.Substring(9, 1) : "";
                //        }
                //        _serverSocket.Close();
                //        _sSocket.Close();
                //        return rtn;
                //    }
                //}


                if (cmd.readwrite == "write") //写
                {
                    var data = cmd.value == "1";
                    var sd = ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                        int.Parse(cmd.address), data); //写数据
                    return "调用成功-" + WriteRead(sd, _serverSocket);
                }
                else //读
                {
                    var sd = ModBus.ModBusRead(int.Parse(cmd.slaveid), ModBus.ModBusFunction.ReadCoils,
                        int.Parse(cmd.address), 1); //读数据
                    var rst = WriteRead(sd, _serverSocket);
                    //解析返回数据
                    return rst.Length == 14 ? rst.Substring(9, 1) : "";
                }
            }
            catch (Exception e)
            {
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, e.Message));
                }
                return e.Message;
            }
        }

        private static string WriteRead(byte[] hexData, Socket serverSocket)
        {
            byte[] recByte = new byte[1024];
            serverSocket.Send(hexData, hexData.Length, 0);
            int bytes = serverSocket.Receive(recByte, recByte.Length, 0);
            var rcv = new byte[bytes];
            Array.Copy(recByte, 0, rcv, 0, bytes);
            var rst = ModBus.HexByteToHexStr(rcv);
            var s = rst.Replace("7777772E7573722E636E", "");//去掉心跳数据
            if (s == "")//排除第一个空
            {
                bytes = serverSocket.Receive(recByte, recByte.Length, 0);
                rcv = new byte[bytes];
                Array.Copy(recByte, 0, rcv, 0, bytes);
                rst = ModBus.HexByteToHexStr(rcv);
                s = rst.Replace("7777772E7573722E636E", "");
            }
            return s;
        }

        /// <summary>
        /// 自动延时控制开闭（仅写寄存器）
        /// (必须值：cmd.SlaveId，cmd.Address)
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string TcpAuto2(Cmd cmd)
        {
            var myListener = new TcpListener(IPAddress.Parse(TcpAddress), int.Parse(TcpPort));
            myListener.Server.ReceiveTimeout = 20000;
            myListener.Start();
            try
            {
                using (var newClient = myListener.AcceptTcpClient())
                {
                    using (var clientStream = newClient.GetStream())
                    {
                        using (var bw = new BinaryWriter(clientStream))
                        {
                            bw.Write(ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                                int.Parse(cmd.address), true));//写开状态
                            Thread.Sleep(string.IsNullOrEmpty(cmd.value) ? int.Parse(Delay) : int.Parse(cmd.value));//cmd.Value替代默认值Delay
                            bw.Write(ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                                int.Parse(cmd.address), false));//写闭状态
                        }
                    }
                }
                myListener.Stop();
                return "调用成功";
            }
            catch (Exception e)
            {
                myListener.Stop();
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, e.Message));
                }
                return e.Message;
            }
        }

        /// <summary>
        /// 读寄存器、手动写寄存器
        /// (必须值：cmd.SlaveId，cmd.Address，cmd.ReadWrite，cmd.Value)
        /// </summary>
        /// <param name="cmd">cmd.ReadWrite="write"，cmd.Value="1"</param>
        /// <returns></returns>
        public static string TcpControll2(Cmd cmd)
        {
            try
            {
                var rtn = "";
                var myListener = new TcpListener(IPAddress.Parse(TcpAddress), int.Parse(TcpPort));
                myListener.Start();
                using (var newClient = myListener.AcceptTcpClient())
                {
                    using (var clientStream = newClient.GetStream())
                    {
                        if (cmd.readwrite == "write")//写
                        {
                            using (var bw = new BinaryWriter(clientStream))
                            {
                                var data = cmd.value == "1";
                                bw.Write(ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil, int.Parse(cmd.address), data));
                            }
                        }
                        else//读
                        {
                            using (var bw = new BinaryWriter(clientStream))
                            {
                                using (var br = new BinaryReader(clientStream))
                                {
                                    bw.Write(ModBus.ModBusRead(int.Parse(cmd.slaveid), ModBus.ModBusFunction.ReadCoils, int.Parse(cmd.address), 1));//先写
                                    rtn = ModBus.HexByteToHexStr(br.ReadBytes(100));//后读
                                }
                            }
                        }
                    }
                }
                myListener.Stop();
                return rtn;
            }
            catch (Exception e)
            {
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, e.Message));
                }
                return e.Message;
            }
        }

        #endregion

        #region 串口通讯区域

        public string FstCotCom()
        {
            using (var port = new SerialPort(comPort))
            {
                port.BaudRate = int.Parse(baudRate);
                port.DataBits = int.Parse(dataBits);
                port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                port.Open();
                var master = ModbusSerialMaster.CreateRtu(port);
                var slaveid = byte.Parse(slaveId);//从站ID
                //1.发送开信号
                WriteValue(master, slaveid, ushort.Parse(registerAdd1), true);//第一个地址由闭变开
                //检查是否开启成功
                if (!ReadValue(master, slaveid, ushort.Parse(registerAdd1)))
                {
                    if (DebugSwitchOn)
                    {
                        Logger.WriteLog(new LogItem(LogType.Error, "FstCot:写true失败！"));
                    }
                    return "开启失败";
                }
                //2.延时
                Thread.Sleep(int.Parse(Delay));
                //3.发送闭信号
                WriteValue(master, slaveid, ushort.Parse(registerAdd1), false);//第一个地址由开变闭
                //检查是否关闭成功
                if (ReadValue(master, slaveid, ushort.Parse(registerAdd1)))
                {
                    if (DebugSwitchOn)
                    {
                        Logger.WriteLog(new LogItem(LogType.Error, "FstCot:写false失败！"));
                    }
                    return "关闭失败";
                }
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "FstCot调用成功！"));
                }
                return "调用成功";
            }
        }

        public string SecCotCom()
        {
            using (var port = new SerialPort(comPort))
            {
                port.BaudRate = int.Parse(baudRate);
                port.DataBits = int.Parse(dataBits);
                port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                port.Open();
                var master = ModbusSerialMaster.CreateRtu(port);
                var slaveid = byte.Parse(slaveId);//从站ID
                //1.发送开信号
                WriteValue(master, slaveid, ushort.Parse(registerAdd2), true);//第二个地址由闭变开
                //检查是否开启成功
                if (!ReadValue(master, slaveid, ushort.Parse(registerAdd2)))
                {
                    if (DebugSwitchOn)
                    {
                        Logger.WriteLog(new LogItem(LogType.Error, "FstCot:写true失败！"));
                    }
                    return "开启失败";
                }
                //2.延时
                Thread.Sleep(int.Parse(Delay));
                //3.发送闭信号
                WriteValue(master, slaveid, ushort.Parse(registerAdd2), false);//第二个地址由开变闭
                //检查是否关闭成功
                if (ReadValue(master, slaveid, ushort.Parse(registerAdd2)))
                {
                    if (DebugSwitchOn)
                    {
                        Logger.WriteLog(new LogItem(LogType.Error, "FstCot:写false失败！"));
                    }
                    return "关闭失败";
                }
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "FstCot调用成功！"));
                }
                return "调用成功";
            }
        }

        public string ThdCotCom()
        {
            using (var port = new SerialPort(comPort))
            {
                port.BaudRate = int.Parse(baudRate);
                port.DataBits = int.Parse(dataBits);
                port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                port.Open();
                var master = ModbusSerialMaster.CreateRtu(port);
                var slaveid = byte.Parse(slaveId);//从站ID
                //1.发送开信号
                WriteValue(master, slaveid, ushort.Parse(registerAdd3), true);//第三个地址由闭变开
                //检查是否开启成功
                if (!ReadValue(master, slaveid, ushort.Parse(registerAdd3)))
                {
                    if (DebugSwitchOn)
                    {
                        Logger.WriteLog(new LogItem(LogType.Error, "FstCot:写true失败！"));
                    }
                    return "开启失败";
                }
                //2.延时
                Thread.Sleep(int.Parse(Delay));
                //3.发送闭信号
                WriteValue(master, slaveid, ushort.Parse(registerAdd3), false);//第三个地址由开变闭
                //检查是否关闭成功
                if (ReadValue(master, slaveid, ushort.Parse(registerAdd3)))
                {
                    if (DebugSwitchOn)
                    {
                        Logger.WriteLog(new LogItem(LogType.Error, "FstCot:写false失败！"));
                    }
                    return "关闭失败";
                }
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "FstCot调用成功！"));
                }
                return "调用成功";
            }
        }

        /// <summary>
        /// 直接读寄存器，不建议使用，仅用于测试
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public string OriginReadCom(Cmd cmd)
        {
            using (var port = new SerialPort(comPort))
            {
                port.BaudRate = int.Parse(baudRate);
                port.DataBits = int.Parse(dataBits);
                port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                port.Open();
                var master = ModbusSerialMaster.CreateRtu(port);
                var slaveid = byte.Parse(slaveId);//从站ID
                //读线圈（读bool）
                //var a = master.ReadCoils(slaveId, startAddress, 1);
                var a = ReadValue(master, slaveid, ushort.Parse(cmd.address));
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error, "OriginRead.ReadValue:地址:" + cmd.address + ",返回:" + a));
                }
                return a.ToString();
            }
        }

        /// <summary>
        /// 直接写寄存器，不建议使用，仅用于测试
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public string OriginWriteCom(Cmd cmd)
        {
            using (var port = new SerialPort(comPort))
            {
                port.BaudRate = int.Parse(baudRate);
                port.DataBits = int.Parse(dataBits);
                port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                port.Open();
                var master = ModbusSerialMaster.CreateRtu(port);
                var slaveid = byte.Parse(slaveId);//从站ID
                //写线圈（写bool）
                //master.WriteSingleCoil(slaveId,startAddress,false);
                WriteValue(master, slaveid, ushort.Parse(cmd.address), bool.Parse(cmd.value));
                if (DebugSwitchOn)
                {
                    Logger.WriteLog(new LogItem(LogType.Error,
                        "OriginWrite.WriteValue:地址:" + cmd.address + ",值:" + cmd.value));
                }
                return "True";
            }
        }

        private void TestCom()
        {
            using (SerialPort port = new SerialPort(comPort))
            {
                // configure serial port配置串口
                port.BaudRate = int.Parse(baudRate);
                port.DataBits = int.Parse(dataBits);
                port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                //port.Parity = Parity.None;
                //port.StopBits = StopBits.One;
                port.Open();
                //创建Modbus主站
                IModbusSerialMaster master = ModbusSerialMaster.CreateRtu(port);

                byte slaveid = byte.Parse(slaveId);//从站ID
                ushort startAddress = ushort.Parse(registerAdd1);//地址16

                //写
                //master.WriteSingleCoil(slaveId,startAddress,false);
                WriteValue(master, slaveid, startAddress, true);
                Console.WriteLine("写入成功。");

                //读
                //var a = master.ReadCoils(slaveId, startAddress, 1);
                var a = ReadValue(master, slaveid, startAddress);
                Console.WriteLine(a);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// 根据从站ID和地址读线圈状态
        /// </summary>
        /// <param name="master"></param>
        /// <param name="slaveid"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static bool ReadValue(IModbusSerialMaster master, byte slaveid, ushort addr)
        {
            return master.ReadCoils(slaveid, addr, 1)[0];
        }

        /// <summary>
        /// 根据从站ID和地址写线圈状态
        /// </summary>
        /// <param name="master"></param>
        /// <param name="slaveid"></param>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public static void WriteValue(IModbusSerialMaster master, byte slaveid, ushort addr, bool value)
        {
            master.WriteSingleCoil(slaveid, addr, value);
        }

        #endregion
    }
}
