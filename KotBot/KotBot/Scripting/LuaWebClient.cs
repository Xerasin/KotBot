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
        [RegisterLuaFunction("Webclient.Encode")]
        public static string Encode(string data)
        {
            return HttpUtility.HtmlEncode(data);
        }
    }
}
