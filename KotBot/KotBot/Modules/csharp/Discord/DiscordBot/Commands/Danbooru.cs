using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;
using System.Net;
using Newtonsoft.Json.Linq;
namespace KotBot.csharp.Discord.DiscordBot.Commands
{
    [Modules.CommandInfo("db", "danbooru", "Discord", "Danbooru!")]
    public class Danbooru : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage)
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Proxy = null;
            string output = "";
            foreach(string str in args)
            {
                output += str;
                output += " ";
            }
            output = output.TrimEnd(' ');
            try
            {
                client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                {
                    if(e.Error != null)
                    {
                        originalMessage.message.Reply($"Failed to find danbooru image for {output}");
                        return;
                    }
                    JToken data = JToken.Parse(e.Result);
                    if (data.GetType() == typeof(JArray))
                    {
                        JArray arrayData = (JArray)data;
                        if (arrayData[0] != null && arrayData[0].GetType() == typeof(JObject))
                        {
                            JObject firstImage = (JObject)arrayData[0];
                            originalMessage.message.Reply($"Random danbooru! https://danbooru.donmai.us/posts/{firstImage["id"]}");
                        }
                        else
                        {
                            originalMessage.message.Reply($"Failed to find danbooru image for {output}");
                        }
                    }
                };
                client.DownloadStringAsync(new Uri($"https://danbooru.donmai.us/posts.json?limit=1&random=true&tags={output}"));
            }
            catch(WebException e)
            {
                originalMessage.message.Reply($"Failed to find danbooru image for {output}");
            }
            return true;
        }

        public override bool ShouldCall(string source)
        {
            if(source == "Discord")
            {
                return true;
            }
            return false;
        }
    }
}
