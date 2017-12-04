using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;
using System.Timers;
using System.Net;
using Newtonsoft.Json.Linq;

namespace KotBot.DiscordBot
{
    public static class MiscDiscord
    {
        public static Random random = new Random();
        public static void Init()
        {
            DiscordManager.DiscordMessage += (DiscordMessageArgs args) =>
            {
                string messageContent = args.Message.Content.ToLower();
                if(!(messageContent.Contains("hi") || messageContent.Contains("hello") || messageContent.Contains("hiya"))) return true;
                bool found = false;
                foreach(var user in args.Message.MentionedUsers)
                {
                    if(user.Id == args.Client.CurrentUser.Id)
                    {
                        found = true;
                    }
                }
                if(!found) return true;
                Task<Discord.Rest.RestUserMessage> dMessage = args.Channel.SendMessageAsync("Hiya! You are looking cute today! hehe *giggles*");
                dMessage.Wait();
                return true;
            };

            DiscordManager.DiscordMessage += (DiscordMessageArgs args) =>
            {
                string messageContent = args.Message.Content.ToLower();
                if(!(messageContent.Contains("ping"))) return true;
                bool found = false;
                foreach(var user in args.Message.MentionedUsers)
                {
                    if(user.Id == args.Client.CurrentUser.Id)
                    {
                        found = true;
                    }
                }
                if(!found) return true;
                Task<Discord.Rest.RestUserMessage> dMessage = args.Channel.SendMessageAsync("pong~ hehe *giggles*");
                dMessage.Wait();
                return true;
            };
            var gameNameTimer = new Timer(10);
            
            gameNameTimer.Elapsed += new ElapsedEventHandler((object e, ElapsedEventArgs args2) => {
                try
                {  
                    gameNameTimer.Interval = 60 * 1000;
                    SetRandomGameName();
                }
                catch (Exception timerFailed)
                {
                    Log.Error(timerFailed.Message);
                }
            });
            gameNameTimer.Start();
        }
        public static void SetRandomGameName()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Proxy = null;
            try
            {
                client.DownloadStringCompleted += (object sender2, DownloadStringCompletedEventArgs e2) =>
                {
                    if(e2.Error != null)
                    {
                        return;
                    }
                    JToken data = JToken.Parse(e2.Result);
                    if (data.GetType() == typeof(JObject))
                    {
                        int results = (int)data["number_of_total_results"];
                        int gameResult = random.Next(0, results - 1);
                        WebClient client2 = new WebClient();
                        client2.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                        client2.Proxy = null;
                        client2.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e3) =>
                        {
                            if(e3.Error != null)
                            {
                                return;
                            }
                            JToken data2 = JToken.Parse(e3.Result);
                            if (data2.GetType() == typeof(JObject))
                            {
                                JObject objectTbl = (JObject)data2;
                                if(objectTbl["results"] != null && objectTbl["results"].GetType() == typeof(JArray))
                                {
                                   
                                    JArray gameResults = (JArray)objectTbl["results"];
                                    if(gameResults.Count > 0)
                                    {
                                        JObject gameTable = (JObject)gameResults[0];
                                        KotBot.DiscordBot.DiscordManager.SetGameName(gameTable["name"].ToString());
                                    }
                                }

                            }
                        };
                        client2.DownloadStringAsync(new Uri($"http://www.giantbomb.com/api/games/?api_key=2613ba373fa9e74dd5d0706e4d5b8b9c3c1a4ab1&format=json&limit=1&offset={gameResult}"));
                    }
                };
                client.DownloadStringAsync(new Uri($"http://www.giantbomb.com/api/games/?api_key=2613ba373fa9e74dd5d0706e4d5b8b9c3c1a4ab1&format=json&limit=1"));
            }
            catch(WebException)
            {
                Log.Error($"Gamename changed failed");
            }
        }
    }
}
