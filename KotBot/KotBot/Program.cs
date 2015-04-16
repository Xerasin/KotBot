using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KotBot
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit +=CurrentDomain_ProcessExit;
            Log.StartLog();
            Scripting.MainLua.PreLoad();
            Scripting.MainLua.LoadAll(true);
            Scripting.LuaHook.Call("StartedUp");
            while(true)
            {
                Scripting.LuaHook.Call("Think");
                Thread.Sleep(50);
                
            }
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //Scripting.LuaAIML.Save();
        }
    }
}
