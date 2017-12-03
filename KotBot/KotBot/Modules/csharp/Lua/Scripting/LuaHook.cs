using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using NLua.Exceptions;

namespace KotBot.Scripting
{
    public static class LuaHook
    {
        public static Dictionary<string, Dictionary<string, LuaFunction>> hooks = new Dictionary<string, Dictionary<string, LuaFunction>>();

        [RegisterLuaFunction("hook.Add", "add a hook", "hook name, name, function")]
        public static void Add(string event_name, string name, LuaFunction func)
        {
            if (!hooks.ContainsKey(event_name))
            {
                hooks[event_name] = new Dictionary<string, LuaFunction>();
            }
            hooks[event_name][name] = func;
        }

        [RegisterLuaFunction("hook.Remove")]
        public static void Remove(string event_name, string name)
        {
            if (!hooks.ContainsKey(event_name))
            {
                return;
            }
            hooks[event_name].Remove(name);
        }
        [RegisterLuaFunction("hook.Call")]
        public static object[] Call(string event_name, params object[] lua_params)
        {
            if (!hooks.ContainsKey(event_name))
            {
                return null;
            }
            Dictionary<string, LuaFunction> t = new Dictionary<string, LuaFunction>();
            foreach (KeyValuePair<string, LuaFunction> key in hooks[event_name])
            {
                t[key.Key] = key.Value;
            }
            ArrayList returns = new ArrayList();
            foreach (KeyValuePair<string, LuaFunction> key in t)
            {
               
                if (key.Value == null)
                {
                    Log.Error("Hook '" + key.Key + "' called a null function");
                    hooks[event_name].Remove(key.Key);
                    continue;
                }
                else
                {
                    try
                    {
                        object[] lua_return =  key.Value.Call(lua_params);
                        returns.Add(lua_return);
                    }
                    catch (LuaException e)
                    {
                        Log.Error("Hook '" + key.Key + "' failed: " + e.Message);
                        hooks[event_name].Remove(key.Key);
                        continue;
                    }
                    catch(System.AccessViolationException)
                    {

                    }
                }
                
            }
            return returns.ToArray();
        }
    }
}
