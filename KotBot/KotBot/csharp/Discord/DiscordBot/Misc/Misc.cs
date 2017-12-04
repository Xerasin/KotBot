﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;

namespace KotBot.DiscordBot
{
    public static class MiscDiscord
    {
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
            var gameNameTimer = new Timer(60 * 1000);
            
            gameNameTimer.Elapsed += new ElapsedEventHandler((object e, ElapsedEventArgs args) => {
                try
                {

                }
                catch (Exception timerFailed)
                {
                    Log.Error(timerFailed.Message);
                }
            });
            gameNameTimer.Start();
    }
    }
}
