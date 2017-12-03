using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace KotBot.Modules
{
    public static class ModuleConfig
    {
        private static Dictionary<string, JObject> Config = new Dictionary<string, JObject>();
        private static string GetSaveFile(string module)
        {
            string filename = null;
            if (string.IsNullOrWhiteSpace(module) || module == "Global")
            {
                filename = "config.cfg";
            }
            else
            {
                filename = $"csharp/{module}/config.cfg";
            }
            return filename;
        }
        private static bool IsLoaded(string module)
        {
            if (string.IsNullOrWhiteSpace(module)) return true;
            return Config.ContainsKey(module);
        }
        private static bool Load(string module)
        {
            string filename = GetSaveFile(module);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    if(!File.Exists(filename))
                    {
                        Config[module] = new JObject();
                        Save(module);
                        return true;
                    }
                    JToken token = JToken.Parse(File.ReadAllText(filename));
                    if (token.GetType() == typeof(JObject))
                    {
                        Config[module] = (JObject)token;
                        return true;
                    }
                    else
                    {
                        Log.Error("Invalid config -- todo");
                    }

                    return false;
                }
                catch(Exception)
                {
                    Log.Error($"{filename} failed to parse?");
                    return false;
                }
            }
            return false;
        }
        private static bool Save(string module)
        {
            if (!IsLoaded(module)) return false;
            string filename = GetSaveFile(module);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                JObject configJson = Config[module];
                try
                {
                    File.WriteAllText(filename, JsonConvert.SerializeObject(configJson, Formatting.Indented));
                    return true;
                }
                catch(Exception ex)
                {
                    Log.Error($"Failed to write save json? {ex.Message}");
                }
                return false;
            }

            return false;
        }
        public static string GetString(string module, string data, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                module = "Global";
            }
            if (string.IsNullOrWhiteSpace(data)) return defaultValue;
            if (!IsLoaded(module))
                Load(module);
            if (IsLoaded(module))
            {
                if (Config[module][data] == null)
                {
                    Config[module][data] = defaultValue;
                    Save(module);
                }
                JToken output;
                if (Config[module].TryGetValue(data, out output))
                {
                    return (string)output.ToString();
                }
                return defaultValue;
            }
            return defaultValue;
        }

        public static T GetValue<T>(string module, string data, T defaultValue) where T: JToken
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                module = "Global";
            }
            if (string.IsNullOrWhiteSpace(data)) return defaultValue;
            if (!IsLoaded(module))
                Load(module);
            if (IsLoaded(module))
            { 
                if(Config[module][data] == null)
                {
                    Config[module][data] = defaultValue;
                    Save(module);
                }
                JToken output;
                if (Config[module].TryGetValue(data, out output))
                {
                    if (typeof(T) == typeof(string))
                        return (T)output.ToString();
                    else if (output.GetType() == typeof(T))
                        return (T)Convert.ChangeType(output, output.GetType());
                }
                return defaultValue;
            }
            return defaultValue;
        }

        public static bool SetValue<T>(string module, string data, T newValue) where T: JToken
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                module = "Global";
            }
            if (string.IsNullOrWhiteSpace(data)) return false;
            if (!IsLoaded(module))
                Load(module);
            if (IsLoaded(module))
            {
                Config[module][data] = newValue;
                return Save(module);
            }
            return false;
        }
    }
}
