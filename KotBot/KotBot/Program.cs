using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KotBot.Modules;
namespace KotBot
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Log.StartLog();

            Modules.ModuleLoader.LoadAllModules();
            while (true)
            {

                //Thread.Sleep(20);
            }
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //Scripting.LuaAIML.Save();
        }
    }
}
