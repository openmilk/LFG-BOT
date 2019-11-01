using Discord.Commands;
using Discord.WebSocket;

using BehaveBot.Classes;
using BehaveBot.Services;


namespace BehaveBot.Modules
{
    using Discord;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static BehaveBot.Services.MessageManager;
    using static DiscordExtensions;


    public class GeneralCommander : ModuleBase<SocketCommandContext>
    {
        private SettingsHandlerService settings;
        private Dictionary<ulong, Dictionary<ulong, channelMesages>> messageHandler;
        private SocketGuildUser user;
        private List<ulong> devs;
        private DiscordSettings.CustomDiscordSetting discordSettings = null;
        private bool isDev = false;

        public GeneralCommander(SettingsHandlerService _settings, MessageManager _messageManager)
        {
            settings = _settings;
            messageHandler = _messageManager.MessageHandler;

            devs = _settings.discord.defaultDiscordSettings.botDevs;


        }

        protected override void BeforeExecute(CommandInfo command)
        {
            if (Context.Guild != null)
            {
                user = Context.User as SocketGuildUser;
            }

            if (Context.Guild != null)
            {
                if (settings.discord.customDiscordSettings.Any(x => x.Key == Context.Guild.Id))
                    discordSettings = settings.discord.customDiscordSettings.First(x => x.Key == Context.Guild.Id).Value;
            }

            isDev = devs.Any(x => x == user.Id);
        }

        [Command("lfg")]
        private async Task lfg()
        {
            var acm = discordSettings.AutoChannelManage;

            if (CheckIfInACM(acm, user))
            {
                await CreateLFGPost(Context.Channel as SocketTextChannel, user.VoiceChannel, user, discordSettings.CustomPrefix);

            }
            await Context.Message.DeleteAsync();
        }

        [Command("lfg")]
        private async Task lfg([Remainder]string RemainingText)
        {
            var acm = discordSettings.AutoChannelManage;

            if (CheckIfInACM(acm, user))
            {
                await CreateLFGPost(Context.Channel as SocketTextChannel, user.VoiceChannel, user, discordSettings.CustomPrefix, RemainingText);
            }
            await Context.Message.DeleteAsync();
        }

        private async Task CreateLFGPost(SocketTextChannel textChnl, SocketVoiceChannel voiceChnl, SocketGuildUser user, string prefix, string text = "")
        {
            var embed = new EmbedBuilder();
            var embedFooter = new EmbedFooterBuilder();

            embedFooter.Text = "Type " + prefix + "lfg message";

            var inviteURL = (await voiceChnl.CreateInviteAsync()).Url;
            var title = $"{voiceChnl.Name} - {voiceChnl.Users.Count()}/{voiceChnl.UserLimit} ";
            var description = "";

            foreach (var a in voiceChnl.Users)
            {
                if (description != "")
                    description += ", ";

                description += a.Mention;
            }

            if (text != "")
                if (text.Count() <= 50)
                    description += "\n" + text + "";
                else
                    description += "\ndescription was to long";


            description += $"\n\n[**Quick Link to " + voiceChnl.Name + "**](" + inviteURL + ")";

            var imageURL = user.GetAvatarUrl();

            embed.WithTitle(title)
                .WithDescription(description)
                .WithThumbnailUrl(imageURL)
                .WithFooter(embedFooter);

            await BuildEmbed(embed, textChnl);

        }

        [Command("open")]
        [Alias("unlock")]
        private async Task Open()
        {
            var acm = discordSettings.AutoChannelManage;

            if (CheckIfInACM(acm, user))
            {
                await user.VoiceChannel.ModifyAsync(x => x.UserLimit = 0);
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Opened your voice channel", messageHandler);
            }
        }



        [Command("reset")]
        private async Task Reset()
        {
            var acm = discordSettings.AutoChannelManage;

            if (CheckIfInACM(acm, user))
            {
                var _acm = GetVoiceSettings(acm, user.VoiceChannel);

                if (_acm == null)
                {
                    var userlimit = _acm.Item3;

                    await user.VoiceChannel.SyncPermissionsAsync();
                    await user.VoiceChannel.ModifyAsync(x => x.UserLimit = userlimit);
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Reset your voice channel", messageHandler);
                }
                else
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Hmm that wasnt ment to happen, you found a bug. DM Milky#0001", messageHandler);
            }
        }

        [Command("lock")]
        private async Task Lock()
        {
            var acm = discordSettings.AutoChannelManage;

            if (CheckIfInACM(acm, user))
            {
                var ammount = user.VoiceChannel.Users.Count();

                await user.VoiceChannel.ModifyAsync(x => x.UserLimit = ammount);
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Locked your voice channel", messageHandler);
            }
        }

        [Command("lock")]
        private async Task Lock(int ammount)
        {
            var acm = discordSettings.AutoChannelManage;

            if (CheckIfInACM(acm, user))
            {
                await user.VoiceChannel.ModifyAsync(x => x.UserLimit = ammount);
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Locked your voice channel", messageHandler);
            }
        }

        [Command("invite")]
        private async Task Invite([Remainder]string RemainingText)
        {
            var acm = discordSettings.AutoChannelManage;

            if (CheckIfInACM(acm, user))
            {
                if (Context.Message.MentionedUsers.Count > 5)
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Only five users at a time", messageHandler);
                else if (Context.Message.MentionedUsers.Count == 0)
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "You didnt mention anyone", messageHandler);
                else
                {
                    var perms = new Discord.OverwritePermissions();

                    perms = perms.Modify(Discord.PermValue.Inherit, Discord.PermValue.Inherit, Discord.PermValue.Inherit, Discord.PermValue.Allow, Discord.PermValue.Allow,
                        Discord.PermValue.Inherit, Discord.PermValue.Inherit, Discord.PermValue.Inherit, Discord.PermValue.Inherit, Discord.PermValue.Allow,
                        Discord.PermValue.Inherit, Discord.PermValue.Inherit, Discord.PermValue.Allow, Discord.PermValue.Allow, Discord.PermValue.Inherit,
                        Discord.PermValue.Inherit, Discord.PermValue.Allow, Discord.PermValue.Allow, Discord.PermValue.Inherit, Discord.PermValue.Inherit);


                    foreach (var a in Context.Message.MentionedUsers)
                    {
                        await user.VoiceChannel.AddPermissionOverwriteAsync(a, perms);
                    }

                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "The users can now join your channel", messageHandler);
                }
            }
        }

        private bool CheckIfInACM(List<Tuple<ulong?, string, int>> ACMs, SocketGuildUser user)
        {
            var bol = false;
            Console.WriteLine("hi");
            if (user.VoiceChannel != null)
            {
                Console.WriteLine("hi");
                if (IsLFGCat(ACMs, user))
                {
                    bol = true;
                    Console.WriteLine("hi");
                }
                else if (IsLFGCat(ACMs, user))
                {
                    bol = true;
                    Console.WriteLine("hi");
                }
                else
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "The voice chat you are in does not support this command", messageHandler);
            }
            else
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "You are not in a voice chat, you need to be in a voice chat to use this command", messageHandler);
            Console.WriteLine("hi");
            return bol;
        }

        public bool IsLFGCat(List<Tuple<ulong?, string, int>> ACMs, SocketGuildUser user)
        {
            Console.WriteLine("hiz");
            foreach (var x in ACMs)
            {
                Console.WriteLine("hiz");
                if (x.Item2 != null)
                {
                    Console.WriteLine("hiz" + user.VoiceChannel.Name.ToLower() + "|" + x.Item2);
                    if (user.VoiceChannel.Name.ToLower().StartsWith(x.Item2))
                        return true;
                }
            }

            return false;
        }

        public bool IsLFGVoice(List<Tuple<ulong?, string, int>> ACMs, SocketGuildUser user)
        {
            if (ACMs.Any(x => x.Item1 == user.VoiceChannel.CategoryId))
            {
                return true;
            }

            return false;
        }

    }
}
