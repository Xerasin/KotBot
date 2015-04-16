using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Scripting;

namespace KotBot
{
    public static class Log
    {
        public static StreamWriter stream;
        public enum LogType
        {
            Error, Warning, Normal, Debug
        }
        public static ConsoleColor GetColor(LogType type)
        {
            switch (type)
            {
                case LogType.Normal:
                    return ConsoleColor.White;
                case LogType.Debug:
                    return ConsoleColor.Cyan;
                case LogType.Warning:
                    return ConsoleColor.Yellow;
                case LogType.Error:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }
        public static void Print(LogType type, params object[] stuff)
        {
            if (stuff == null)
                return;
            Console.ForegroundColor = GetColor(type);
            Console.Write(DateTime.Now.TimeOfDay.ToString() + " - ");
            if (stream != null) stream.Write(DateTime.Now.TimeOfDay.ToString() + " - ");
            for (int I = 0; I < stuff.Length; I++)
            {
                if (I != 0)
                {
                    Console.Write("\t");
                    if (stream != null) stream.Write("\t");
                }
                Console.Write(stuff[I]);
                if (stream != null) stream.Write(stuff[I]);
            }
            Console.Write("\n");
            if (stream != null) stream.Write("\r\n");
        }
        [RegisterLuaFunction("debug")]
        public static void Debug(params object[] stuff)
        {
            Print(LogType.Debug, stuff);
        }
        [RegisterLuaFunction("error")]
        public static void Error(params object[] stuff)
        {
            Print(LogType.Error, stuff);
        }
        [RegisterLuaFunction("print")]
        public static void Print(params object[] stuff)
        {
            Print(LogType.Normal, stuff);
        }
        [RegisterLuaFunction("warning")]
        public static void Warning(params object[] stuff)
        {
            Print(LogType.Warning, stuff);
        }
        public const string LogFolder = "logs";
        public const string LogFile = "log";
        public const string FileExt = ".txt";
        public static void StartLog()
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
            string date = String.Format("{0}-{1}-{2}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year);
            string id = "";
            for (int I = 0; I <= 100; I++)
            {
                string file = LogFolder + "/" + LogFile + "." + date + "." + I.ToString() + FileExt;

                if (!File.Exists(file))
                {
                    id = file;
                    break;
                }
            }
            stream = new StreamWriter(id);
            stream.AutoFlush = true;
        }
        public static void EndLog()
        {
            stream.Close();
        }
    }
}
