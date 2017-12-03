using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using NLua;
using NLua.Exceptions;

namespace KotBot.Scripting
{
    public static class MainLua
    {
        public static Lua LuaInstance;
        public static Lua OldLuaInstance;
        private static Dictionary<string, object> PermaLocals = new Dictionary<string, object>();
        private static void SetupFunction(string luaname)
        {
            string[] path = luaname.Split('.');
            string table = "";
            for(int I = 0; I < path.Length - 1; I++)
            {
                
                string cur_table = path[I];
                table += (I != 0 ? "." : "") + cur_table;
                object cTable = LuaInstance[table];
                if(cTable == null)
                {
                 LuaInstance.NewTable(table);
                }
            }
        }
        public static void RegisterLuaFunction(object c, string methodname, string luaname)
        {
            Type pPrgType = (c).GetType();
            MethodInfo mInfo = pPrgType.GetMethod(methodname);
            SetupFunction(luaname);
            LuaInstance.RegisterFunction(luaname, c, mInfo);
        }
        public static void RegisterLuaFunction(Type c, MethodInfo mInfo, string luaname)
        {
            SetupFunction(luaname);
            LuaInstance.RegisterFunction(luaname, c, mInfo);
        }
        public static void RegisterLuaFunction(Type c, string methodname, string luaname)
        {
            SetupFunction(luaname);
            Type pPrgType = c;
            MethodInfo mInfo = pPrgType.GetMethod(methodname);
            LuaInstance.RegisterFunction(luaname, c, mInfo);
        }
        public static void PreLoad()
        {
            if (OldLuaInstance != null)
            {
                OldLuaInstance.Dispose();
            }
            OldLuaInstance = LuaInstance;
            LuaInstance = new Lua();
            registerAttributesFromClass(typeof(LuaTimer));
            registerAttributesFromClass(typeof(LuaHook));
            registerAttributesFromClass(typeof(MainLua));
            registerAttributesFromClass(typeof(Log));
            registerAttributesFromClass(typeof(LuaJSON));
            registerAttributesFromClass(typeof(LuaWebClient));
            registerAttributesFromClass(typeof(LuaTask));
        }
        public static void LoadAll(bool firstload)
        {
            IncludeFolder("modules");
            IncludeFolder("autorun");
            if (firstload)
                IncludeFolder("autorun/firstrun");
        }

        [RegisterLuaFunction("bot.reload")]
        public static void ReloadBot()
        {
            LuaTimer.RemoveAllTimers();
            LuaHook.hooks = new Dictionary<string, Dictionary<string, LuaFunction>>();
            Scripting.MainLua.PreLoad();
            Scripting.MainLua.LoadAll(false);
            Scripting.LuaHook.Call("StartedUp");
        }
        /*[RegisterLuaFunction("RealTime")]
        public static float Time()
        {
            return Util.Time;
        }*/
        //int last = 0;
        public static LuaTable GetNewTable()
        {
            LuaInstance.NewTable("t");
            return (LuaTable)LuaInstance["t"];
        }
        private static void ReadInternalLua(string lua)
        {
            Assembly _assembly;
            _assembly = Assembly.GetExecutingAssembly();
            string assembly = _assembly.GetName().FullName;
            Stream stream = _assembly.GetManifestResourceStream("Treg_Engine"+"."+lua);
            StreamReader reader = new StreamReader(stream);
            object test;
            DoString(reader.ReadToEnd(), out test);
        }
        private static Random luaRandom = new Random();
        [RegisterLuaFunction("math.randomseed")]
        public static void RandomSeed(int random)
        {
            luaRandom = new Random(random);
        }
        [RegisterLuaFunction("math.random")]
        public static double RandomSeed(int? start = null, int? end = null)
        {
            if(end == null)
            {
                if(start == null)
                {
                    return luaRandom.NextDouble();
                }
                return luaRandom.Next((int)start);
            }
            return luaRandom.Next((int)start, (int)end + 1);
        }
        [RegisterLuaFunction("runfolder")]
        public static void IncludeFolder(string folder)
        {
            try
            {
                foreach (string file in Directory.GetFiles(@"lua\"+folder))
                {
                    try
                    {
                        LuaInstance.DoFile(file);
                    }
                    catch (LuaException e)
                    {
                        Log.Error(e.Message + " : " + e.InnerException.Message);
                    }
                    System.Threading.Thread.Sleep(5);
                }
            }
            catch (TargetException e)
            {
                Log.Error(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Log.Error(e.Message);
            }
        }
       

        private static Dictionary<string, object> MergeTab(Dictionary<string, object> table1, Dictionary<string, object> table2)
        {
            Dictionary<string, object> NewTable = new Dictionary<string,object>();
            foreach (KeyValuePair<string, object> entry in table1)
            {
                NewTable[entry.Key] = entry.Value;
            }
            foreach (KeyValuePair<string, object> entry in table2)
            {
                NewTable[entry.Key] = entry.Value;
            }
            return NewTable;
        }

        public static void AddPermaLocal(string key, object value)
        {
            if (value == null && PermaLocals.ContainsKey(key))
            {
                PermaLocals.Remove(key);
            }
            else
            {
                PermaLocals[key] = value;
            }
           
        }
        public static void registerAttributesFromClass(Type pTargetType)
        {
            foreach (MethodInfo method in pTargetType.GetMethods())
            {
                foreach (Attribute attribute in Attribute.GetCustomAttributes(method))
                {
                    if (attribute.GetType() == typeof(RegisterLuaFunction))
                    {
                        RegisterLuaFunction function = (RegisterLuaFunction)attribute;
                        RegisterLuaFunction(pTargetType, method, function.name);
                    }
                }
            }
        }
        public static void print(string text)
        {

        }
        [RegisterLuaFunction("dolua")]
        public static bool DoString(string code, out object returns, params object[] t)
        {
            Dictionary<string, object> stuff = new Dictionary<string, object>();
            if (t.Length % 2 == 0)
            {
                for (int I = 0; I < t.Length; I += 2)
                {
                    object key = t[I];
                    object value = t[I + 1];
                    if (key.GetType() == typeof(string))
                    {
                        stuff[(string)key] = value;
                    }
                }
                return DoString(code, stuff, out returns);
            }
            else
            {
                throw new Exception("Invalid arguments");
            }
        }

        public static bool DoString(string code, Dictionary<string, object> locals, out object returns)
        {
            string header = "";
            Dictionary<string, object> newLocals = MergeTab(PermaLocals, locals);
            foreach (KeyValuePair<string, object> local in newLocals)
            {
                LuaInstance[local.Key] = local.Value;
                header += "local " + local.Key + " = " + local.Key + " ";
            }
            try
            {
                object[] test = LuaInstance.DoString(header + "\n" + code);
                if (test != null && test.Length > 0)
                {
                    returns = test[0];
                }
                else
                {
                    returns = null;
                }
                
                return true;
            }
            catch(LuaException e)
            {
                
                Scripting.LuaHook.Call("Error", e);
                returns = e.Message;
                return false;
            }

            /*foreach (KeyValuePair<string, object> local in newLocals)
            {
                //LuaInstance[local.Key] = null;
            }*/
        }
    }
}