using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;
using KotBot.Scripting;
using System.Reflection;

namespace KotBot.DiscordBot
{
    public static class LuaDiscord
    {
        public static void LoadAll(Assembly assembly)
        {
            if(assembly != null)
            {
                var pluginType = assembly.GetType("KotBot.Scripting.MainLua");
                if(pluginType != null)
                {
                    var methods = pluginType.GetMethods();
                    var methodInfo = pluginType.GetMethod("registerAttributesFromClass", BindingFlags.Static | BindingFlags.Public);
                    if(methodInfo != null)
                    {
                        var outMain = methodInfo.Invoke(null, new[] { typeof(KotBot.DiscordBot.DiscordManager) });
                    }
                    else
                    {
                        Log.Error("Is Lua Module broken?");
                    }
                }
                else
                {
                    Log.Error("Is Lua Module broken?");
                }
            }
        }
    }
}