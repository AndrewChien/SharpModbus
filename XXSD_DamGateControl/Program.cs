using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Threading;
using Modbus.Device;

namespace XXSD_DamGateControl
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new CallService()
            };
            ServiceBase.Run(ServicesToRun);

            
            //TestSck();

            //Cmd cmd = new Cmd();
            ////读
            //cmd.SlaveId = "6";
            //cmd.Address = "16";
            //cmd.ReadWrite = "read";
            //cmd.Value = "0";
            //Console.WriteLine(TcpControll(cmd));
            //Console.ReadKey();
        }

        public static string TcpControll(Cmd cmd)
        {
            try
            {
                var rtn = "";
                var ipe = new IPEndPoint(IPAddress.Parse("172.16.10.40"), 2317);
                var sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sSocket.Bind(ipe);
                sSocket.Listen(0);
                var serverSocket = sSocket.Accept();
                string rst = "";
                if (cmd.readwrite == "write")//写
                {
                    var data = cmd.value == "1";
                    var sd = ModBus.ModBusWrite(int.Parse(cmd.slaveid), ModBus.ModBusFunction.WriteCoil,
                        int.Parse(cmd.address), data);//写数据
                    rtn = "调用成功-" + WriteRead(sd, serverSocket);
                }
                else//读
                {
                    var sd = ModBus.ModBusRead(int.Parse(cmd.slaveid), ModBus.ModBusFunction.ReadCoils,
                        int.Parse(cmd.address), 1);//读数据
                    rst = WriteRead(sd, serverSocket);
                    //解析返回数据
                    rtn = rst.Length == 14 ? rst.Substring(9, 1) : "";
                }
                serverSocket.Close();
                sSocket.Close();
                return rtn;
            }
            catch (Exception e)
            {
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
            var s = rst.Replace("7777772E7573722E636E", "");
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

        public static void TestSck()
        {
            int port = 2317;
            string host = "192.168.12.73";
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            Socket sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sSocket.Bind(ipe);
            sSocket.Listen(0);
            Console.WriteLine("监听已经打开，请等待");
            Socket serverSocket = sSocket.Accept();
            Console.WriteLine("连接已经建立");
            
            byte[] recByte = new byte[1024];
            int bytes = 0;
            string rst = "";

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("写开回传:{0}", WriteRead("06 05 00 10 FF 00 8C 48", serverSocket));
                Thread.Sleep(1000);
                Console.WriteLine("读开回传:{0}", WriteRead("060100100001FDB8", serverSocket));
                Thread.Sleep(1000);
                Console.WriteLine("写闭回传:{0}", WriteRead("06 05 00 10 00 00 CD B8", serverSocket));
                Thread.Sleep(1000);
                Console.WriteLine("读闭回传:{0}", WriteRead("060100100001FDB8", serverSocket));
            }
            
            serverSocket.Close();
            sSocket.Close();
            Console.ReadKey();
        }

        private static string WriteRead(string hexdata,Socket serverSocket)
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
        
        public static void TestModbus()
        {
            var myListener = new TcpListener(IPAddress.Parse("172.16.10.40"), 2317);
            myListener.Start();
            var newClient = myListener.AcceptTcpClient();
            Console.WriteLine("连接成功");
            NetworkStream clientStream = newClient.GetStream();
            var bw = new BinaryWriter(clientStream);
            //写入
            bw.Write(ModBus.ModBusWrite(6, ModBus.ModBusFunction.WriteCoil, 16, true));
            Console.WriteLine("开启");
            Thread.Sleep(5000);
            bw.Write(ModBus.ModBusWrite(6, ModBus.ModBusFunction.WriteCoil, 16, false));
            Console.WriteLine("关闭");
            Console.ReadKey();
        }

        /// <summary>
        /// socket方式
        /// </summary>
        public static void TestSocket()
        {
            var socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse("172.16.10.40"), 2317);
            socketWatch.Bind(endpoint);
            socketWatch.Listen(20);
            Socket socConnection = socketWatch.Accept();
            socConnection.Send(ModBus.HexStrToHexByte("06 05 00 10 FF 00 8C 48"));
            Thread.Sleep(5000);
            socConnection.Send(ModBus.HexStrToHexByte("06 05 00 10 00 00 CD B8"));
            Console.ReadKey();
        }
    }
}
