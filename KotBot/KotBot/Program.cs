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
            Scripting.MainLua.PreLoad();
            Scripting.MainLua.LoadAll();
            while(true)
            {
                Scripting.LuaHook.Call("Think");
                Thread.Sleep(50);
                
            }
        }
    }
}
