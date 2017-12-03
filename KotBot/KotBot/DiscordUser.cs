using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
namespace KotBot.DiscordBot
{
    public class DiscordUser : User
    {
        public Discord.User member;
        public DiscordUser(Discord.User user)
        {
            this.member = user;
        }

        public Discord.User GetMember()
        {
            return member;
        }
        public override string GetName()
        {
            return member.Name;
        }
        public override string ProcessMessage(string message)
        {
            message = message.Replace("{name}", this.GetName());
            message = message.Replace("{aname}", string.Format("<@{0}>", this.member.Id));
            return message;
        }
        public async override void Message(string message)
        {
            message = this.ProcessMessage(message);
            Discord.Channel channel = await DiscordManager.client.CreatePrivateChannel(member.Id);
            await channel.SendMessage(message);
            Discord.User currentUser = channel.GetUser(DiscordManager.client.CurrentUser.Id);
            Scripting.LuaHook.Call("MessageSent", new Message(new DiscordPMClient(currentUser, this.member), new DiscordUser(currentUser), message));
        }
        public override string GetUserID()
        {
            return "|Discord|" + member.Id;
        }
    }
}
