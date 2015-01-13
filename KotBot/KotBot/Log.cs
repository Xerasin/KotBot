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
        public static void Debug(params object[] stuff)
        {
            Print(LogType.Debug, stuff);
        }
        public static void Error(params object[] stuff)
        {
            Print(LogType.Error, stuff);
        }
        [RegisterLuaFunction("print")]
        public static void Print(params object[] stuff)
        {
            Print(LogType.Normal, stuff);
        }
        public static void Warning(params object[] stuff)
        {
            Print(LogType.Warning, stuff);
        }
        public const string LogFile = "log";
        public const string FileExt = ".txt";
        public static void StartLog()
        {
            for (int I = 3; I >= 0; I--)
            {
                string file = LogFile + (I == 0 ? "" : I.ToString()) + FileExt;

                if (File.Exists(file))
                {
                    if (I == 3)
                    {
                        File.Delete(file);
                    }
                    else
                    {
                        File.Move(file, LogFile + (I + 1) + FileExt);
                    }
                }
            }
            string filename = LogFile + FileExt;
            stream = new StreamWriter(filename);
            stream.AutoFlush = true;
        }
        public static void EndLog()
        {
            stream.Close();
        }
    }
}
