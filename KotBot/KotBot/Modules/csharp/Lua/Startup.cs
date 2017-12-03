using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KotBot;
using KotBot.Modules;
using Newtonsoft.Json.Linq;

namespace Plugin
{
    class Plugin
    {
        [ModuleInfo("Lua KotBot", "0.0.5", "Katherine Loveland", "Lua Functionality to KotBot")]
        public static void Main(string[] args)
        {
            KotBot.Scripting.MainLua.PreLoad();
            KotBot.Scripting.MainLua.LoadAll(false);
        }
        public static void Close()
        {

        }

        static bool IsActive()
        {
            return true;
        }
    }
}
