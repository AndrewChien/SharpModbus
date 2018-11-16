using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XXSD_DamGateControl
{
    /// <summary>
    /// 错误日志类，用于错误信息处理，包括错误、警告、信息，分为系统事件日志，文本文件日志两种
    /// </summary>
    public class LogHelper
    {
        #region 增加单例模式 add：钱春

        private static LogHelper instance;
        private static object _lock = new object();
        private LogHelper()
        {

        }
        public static LogHelper GetInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new LogHelper();
                    }
                }
            }
            return instance;
        }

        #endregion

        #region ErrLog属性定义
        /// <summary>
        /// 文本文件的名称，文本文件日志有效
        /// </summary>
        public string LogPrefix = "Log";
        List<LogItem> List = new List<LogItem>();
        #endregion

        public void Clear()
        {
            List.Clear();
        }
        public override string ToString()
        {
            string result = string.Empty;
            //foreach (var item in List)
            //{
            //    result += item.ToShortString() + "\r\n";
            //}
            for (int i = List.Count - 1; i >= 0; i--)
            {
                var item = List[i];
                result += item.ToShortString() + "\r\n";
            }
            return result.TrimEnd(new char[] { '\r', '\n' });
        }

        /// <summary>
        /// 日志记录信息函数
        /// </summary>
        /// <param name="message">具体的信息</param>
        public void WriteLog(LogItem message)
        {
            WriteTxtLog(message);
        }

        /// <summary>
        /// 文本文件日志记录信息函数
        /// </summary>
        /// <param name="message">具体的信息</param>
        public void WriteTxtLog(LogItem message)
        {
            try
            {
                lock (LogPrefix)
                {
                    List.Add(message);
                    if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\"))
                        Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\");
                    using (StreamWriter sw = File.AppendText(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\" + LogPrefix + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                    {
                        sw.WriteLine(message.ToString());
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public class LogItem
    {
        public LogType Type;
        public DateTime Time;
        public string Message;
        /// <summary>
        /// 快捷的记录日志内容
        /// </summary>
        /// <param name="message">日志内容</param>
        public LogItem(string message)
        {
            Type = LogType.Information;
            Time = DateTime.Now;
            Message = message;
        }
        /// <summary>
        /// 记录日志类型和内容
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="message">日志内容</param>
        public LogItem(LogType type, string message)
        {
            Type = type;
            Time = DateTime.Now;
            Message = message;
        }
        /// <summary>
        /// 记录完整的日志日志
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="time">日志时间</param>
        /// <param name="message">日志内容</param>
        public LogItem(LogType type, DateTime time, string message)
        {
            Type = type;
            Time = time;
            Message = message;
        }
        public override string ToString()
        {
            string result = string.Empty;
            switch (Type)
            {
                case LogType.Information:
                    result = "信息：[" + Time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]" + Message;
                    break;
                case LogType.Warnning:
                    result = "警告：[" + Time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]" + Message;
                    break;
                case LogType.Error:
                    result = "错误：[" + Time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]" + Message;
                    break;
            }
            return result;
        }
        public string ToShortString()
        {
            string result = string.Empty;
            switch (Type)
            {
                case LogType.Information:
                    result = "信息：[" + Time.ToString("HH:mm:ss.fff") + "]" + Message;
                    break;
                case LogType.Warnning:
                    result = "警告：[" + Time.ToString("HH:mm:ss.fff") + "]" + Message;
                    break;
                case LogType.Error:
                    result = "错误：[" + Time.ToString("HH:mm:ss.fff") + "]" + Message;
                    break;
            }
            return result;
        }
    }

    public enum LogType
    {
        Information = 0,
        Warnning = 1,
        Error = 2,
    }
}
