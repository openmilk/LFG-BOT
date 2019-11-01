using System.Threading.Tasks;


using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BehaveBot.Classes
{
    using BehaveBot.Services;
    using Discord.Net;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    public static class DiscordExtensions
    {
        public static string UrlImageType(string url)//used to see the filetype
        {
            var dot = url.LastIndexOf('.');
            return url.Substring(dot);
        }

        public async static Task BuildEmbed(EmbedBuilder embed, SocketTextChannel Channel)
        {
            await Channel.SendMessageAsync("", false, embed.Build());
        }

        public static async Task<IUserMessage> TrySendDMAsync(this IUser user, string message = null, Embed embed = null)
        {
            try
            {
                return await user.SendMessageAsync(message, embed: embed);
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
            {
                return null;
            }
        }

        public static bool IsAdmin(SocketGuildUser user, bool isDev)
        {
            var isAdmin = false;

            if (isDev)
                isAdmin = true;

            else if (user.GuildPermissions.Administrator == true)
                isAdmin = true;

            return isAdmin;
        }


        public static Tuple<ulong?, string, int> GetVoiceSettings(List<Tuple<ulong?, string, int>> AutoChannelManage, SocketVoiceChannel voiceChnl)
        {
            Tuple<ulong?, string, int> settings = null;

            var prefix = AutoChannelManage.Where(x => x.Item2 != null);
            var cat = AutoChannelManage.Where(x => x.Item1 != null);

            if (prefix.Count() != 0)
            try
            {
                settings = prefix.First(x => voiceChnl.Name.ToLower().StartsWith(x.Item2));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception VC prefix Message: " + ex.Message);
            }

            if(settings == null && cat.Count() != 0)
                try
                {
                    cat.First(x => x.Item1 == voiceChnl.CategoryId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception Cat Message: " + ex.Message);
                }
            
            


            return settings;
        }


    }
}
