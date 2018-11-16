using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace XXSD_DamGateControl
{    /// <summary>
    /// 系统配置类
    /// </summary>
    [Serializable]
    public class Configuration
    {
        //COM口
        public string ComPort;
        //波特率
        public string BaudRate;
        //数据位
        public string DataBits;
        //校验位
        public string Parity;
        //停止位
        public string StopBits;
        //调试开关
        public string DebugSwitch;
        //从站ID
        public string SlaveId;
        //寄存器地址1
        public string RegisterAdd1;
        //寄存器地址2
        public string RegisterAdd2;
        //寄存器地址3
        public string RegisterAdd3;
        //延时（毫秒）
        public string Delay;
        //本服务用地址
        public string PublishHost;
        //本服务用端口
        public string PublishPort;
        public string TcpAddress;
        public string TcpPort;
        public string SocketDelay;
    }

    //public enum Parity
    //{
    //    None,//0
    //    Odd,//1
    //    Even,//2
    //    Mark,//3
    //    Space,//4
    //}

    //public enum StopBits
    //{
    //    None,//0
    //    One,//1
    //    Two,//2
    //    OnePointFive,//3
    //}

    public class Cmd
    {
        public string command;
        public string address;
        public string value;
        public string slaveid;
        public string readwrite;
    }

    public class Rst
    {
        public string Result;
    }

    public struct PocRunPar
    {
        public byte UdpHbCnt;
        public byte TcpHbCnt;
    }

    public class ServerMsg
    {
        public string uid;
        public string username;
        public string defaultgroup;
        public List<GroupMsg> groups;
    }

    public class GroupMsg
    {
        public string gid;
        public string gname;
        public string numbers;
        public List<UserMsg> users;
    }

    public class UserMsg
    {
        public string uid;
        public string online;
        public string uname;
    }
}
