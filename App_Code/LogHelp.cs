using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.IO;

namespace Commen
{
    public class LogHelp
    {
        public static String AppPath = HttpRuntime.AppDomainAppPath + "App_Data\\";
        public static String FileName = DateTime.Now.ToString("yyyy-MM-dd") + " log.txt";

        /// <summary>
        /// 输出函数，把要输出的数据写入文件
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="msg">输出的信息</param>
        /// <param name="path">引用的路径</param>
        public static void Log(String logName, String msg, string path)
        {
            if (!Directory.Exists(AppPath))
            {
                Directory.CreateDirectory(AppPath);
            }
            String FilePath = AppPath + FileName;
            try
            {
                StreamWriter strmW = new StreamWriter(FilePath, true, System.Text.Encoding.UTF8);
                strmW.WriteLine(DateTime.Now.ToString("[hh:mm:ss] [") + logName + "]  " + msg + " " + path);
                strmW.Close();
            }
            catch
            {
            }
        }

        public static void DebugLog(object obj)
        {
            Log("Debug", obj.ToString(), "");
        }

        /// <summary>
        /// 输出对象到文件
        /// </summary>
        /// <param name="obj"></param>
        public static void Log(object obj)
        {
            Log("Info", obj.ToString(), "");
        }

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="msg">警告信息</param>
        /// <param name="path">发生警告信息位置的路径</param>
        public static void LogWarningMsg(string msg, string path)
        {
            Log("Wornning", msg, path);
        }

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="msg">错误信息</param>
        /// <param name="path">发生错误信息位置的路径</param>
        public static void LogErrorMsg(string msg, string path)
        {
            Log("Error", msg, path);
        }

        public static void GetFun()
        {
            //得到函数名：
            var st1 = new StackTrace();
            string fun = st1.GetFrame(0).ToString();
            ///获取调用此的函数参数
            ParameterInfo[] parameters = st1.GetFrame(1).GetMethod().GetParameters();
            //得到代码行，源代码文件名：
            StackTrace st = new StackTrace(new StackFrame(true));
            Console.WriteLine(" Stack trace for current level: {0}", st);

            StackFrame sf = st.GetFrame(0);
            Console.WriteLine(" File: {0}", sf.GetFileName());
            Console.WriteLine(" Method: {0}", sf.GetMethod().Name);
            Console.WriteLine(" Line Number: {0}", sf.GetFileLineNumber());
            //Console.WriteLine(" Column Number: {0}", sf.GetFileColumnNumber());
        }
    }
}
