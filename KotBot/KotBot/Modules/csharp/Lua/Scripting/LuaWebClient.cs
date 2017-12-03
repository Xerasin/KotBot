using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Collections.Specialized;
namespace KotBot.Scripting
{
    public static class LuaWebClient
    {
        static List<WebClient> clients = new List<WebClient>();

        [RegisterLuaFunction("Webclient.Create")]
        public static WebClient Create()
        {
            WebClient client = new WebClient();
            client.Proxy = null;
            return client;
        }
        [RegisterLuaFunction("Webclient.FetchAsync")]
        public static void Fetch(string url, NLua.LuaFunction func, NLua.LuaFunction errorfunc = null, string cookies = "")
        {
            WebClient client = Create();
            client.Headers.Add(HttpRequestHeader.Cookie, cookies);
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.DownloadStringCompleted += (sender, data) =>
            {
                if (!data.Cancelled && data.Error == null)
                {
                    try
                    {
                        func.Call(data.Result);
                    }
                    catch(NLua.Exceptions.LuaException except)
                    {
                        Log.Error(except.Message);
                    }
                }
                else
                {
                    if(errorfunc != null)
                    {
                        try
                        {
                            errorfunc.Call(data.Error.Message);
                        }
                        catch (NLua.Exceptions.LuaException except)
                        {
                            Log.Error(except.Message);
                        }
                    }
                }
                client.Dispose();
            };
            client.DownloadStringAsync(new System.Uri(url));
            clients.Add(client);
        }
        [RegisterLuaFunction("Webclient.PostAsync")]
        public static void Post(string url, NLua.LuaTable postvars, NLua.LuaFunction func, NLua.LuaFunction errorfunc = null, string cookies = "")
        {
            WebClient client = Create();
            client.Headers.Add(HttpRequestHeader.Cookie, cookies);
            client.UploadValuesCompleted += (sender, data) =>
            {
                if (!data.Cancelled && data.Error == null)
                {
                    try
                    {
                        func.Call(System.Text.Encoding.UTF8.GetString(data.Result));
                    }
                    catch (NLua.Exceptions.LuaException except)
                    {
                        Log.Error(except.Message);
                    }
                }
                else
                {
                    if (errorfunc != null)
                    {
                        try
                        {
                            errorfunc.Call(data.Error.Message);
                        }
                        catch (NLua.Exceptions.LuaException except)
                        {
                            Log.Error(except.Message);
                        }
                    }
                }
                client.Dispose();
            };
            NameValueCollection collection = new NameValueCollection();
            foreach (var key in postvars.Keys)
            {
                object value = postvars[key];
                if (key.GetType() == typeof(string) && value.GetType() == typeof(string))
                {
                    collection[(string)key] = (string)value;
                }
                
            }
            client.UploadValuesAsync(new System.Uri(url), collection);
            clients.Add(client);
        }
        public static void DisposeAllClients()
        {
            foreach (WebClient item in clients.ToArray())
            {
                item.Dispose();
            }
            clients = new List<WebClient>();
        }
        [RegisterLuaFunction("Webclient.Encode")]
        public static string Encode(string data)
        {
            return HttpUtility.UrlEncode(data);
        }
        [RegisterLuaFunction("Webclient.Decode")]
        public static string Decode(string data)
        {
            return HttpUtility.UrlDecode(data);
        }
    }
}
