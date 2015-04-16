using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLua;
namespace KotBot.Scripting
{
    public static class LuaJSON
    {
        [RegisterLuaFunction("json.parse")]
        public static LuaTable Parse(string json)
        {
            try
            {
                return JObjectToLuaTable(JObject.Parse(json));
            }
            catch(Newtonsoft.Json.JsonReaderException e)
            {
                return null;
            }
        }
        [RegisterLuaFunction("json.tostring")]
        public static string GetJson(LuaTable table)
        {
            return GetJObjectFromLuaTable(table).ToString();
        }
        
        public static JObject GetJObjectFromLuaTable(LuaTable table)
        {
            JObject obj = new JObject();
            foreach (var x in table.Keys)
            {
                object value = table[x];
                if (value.GetType() == typeof(LuaTable))
                {
                    obj[x.ToString()] = GetJObjectFromLuaTable((LuaTable)value);
                }
                else if (value.GetType() == typeof(double))
                {
                    obj[x.ToString()] = (double)value;
                }
                else if (value.GetType() == typeof(bool))
                {
                    obj[x.ToString()] = (bool)value;
                }
                else
                {
                    obj[x.ToString()] = value.ToString();
                }
            }
            return obj;
        }
        private static object ConvertJToken(JToken test)
        {
            if (test.Type == JTokenType.Float || test.Type == JTokenType.Integer)
            {
                return Convert.ToDouble(test.ToString());
            }
            else if (test.Type == JTokenType.Boolean)
            {
                return Convert.ToBoolean(test.ToString());
            }
            else if (test.Type == JTokenType.String)
            {
                return test.ToString();
            }
            else if (test.Type == JTokenType.Object)
            {
                return JObjectToLuaTable((JObject)test);
            }
            else if (test.Type == JTokenType.Array)
            {
                return JArrayToLuatable((JArray)test);
            }
            else
            {
                return null;
            }
        }
        private static LuaTable JArrayToLuatable(JArray jarray)
        {
            LuaTable tab2 = MainLua.GetNewTable();
            for (int I = 0; I < jarray.Count; I++)
            {
                tab2[I + 1] = ConvertJToken(jarray[I]);
            }
            return tab2;
        }
        private static LuaTable JObjectToLuaTable(JObject jobj)
        {
            LuaTable tab = MainLua.GetNewTable();
            foreach(var x in jobj)
            {
                if (x.Value.Type == JTokenType.Object)
                {
                    tab[x.Key.Replace(".","\\.")] = JObjectToLuaTable((JObject)x.Value);
                }
                else if (x.Value.Type == JTokenType.Array)
                {
                    tab[x.Key.Replace(".", "\\.")] = JArrayToLuatable((JArray)x.Value);
                }
                else
                {
                    tab[x.Key.Replace(".", "\\.")] = ConvertJToken(x.Value);
                }
            }
            return tab;
        }
    }
}
