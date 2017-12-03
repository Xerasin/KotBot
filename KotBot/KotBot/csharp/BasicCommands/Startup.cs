using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KotBot.Modules;

namespace Plugin
{
    public class Plugin
    {
        [ModuleInfo("Basic Commands", "0.0.5", "Katherine Loveland", "Basic Commands")]
        public static void Main(string[] args)
        {
            
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
