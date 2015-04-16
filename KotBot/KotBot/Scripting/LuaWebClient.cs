using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
namespace KotBot.Scripting
{
    public static class LuaWebClient
    {
        [RegisterLuaFunction("Webclient.Create")]
        public static WebClient Create()
        {
            WebClient client = new WebClient();
            client.Proxy = null;
            return client;
        }
        [RegisterLuaFunction("Webclient.FetchAsync")]
        public static void Fetch(string url, NLua.LuaFunction func, NLua.LuaFunction errorfunc = null)
        {
            WebClient client = Create();
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
        }
        [RegisterLuaFunction("Webclient.Encode")]
        public static string Encode(string data)
        {
            return HttpUtility.HtmlEncode(data);
        }
    }
}
