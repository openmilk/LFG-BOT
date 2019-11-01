using Discord.Commands;
using Discord.WebSocket;

using BehaveBot.Classes;
using BehaveBot.Services;

using System.Linq;
using System.Collections.Generic;


namespace BehaveBot.Modules
{
    using System;
    using System.Threading.Tasks;
    using static BehaveBot.Services.DiscordSettings.CustomDiscordSetting;
    using static BehaveBot.Services.MessageManager;
    using static DiscordExtensions;
    using static StringExtensions;


    public class ModCommander : ModuleBase<SocketCommandContext>
    {
        private SettingsHandlerService settings;
        private Dictionary<ulong, Dictionary<ulong, channelMesages>> messageHandler;
        private SocketGuildUser user;
        private List<ulong> devs;
        private DiscordSettings.CustomDiscordSetting discordSettings = null;
        private bool isDev = false;

        public ModCommander(SettingsHandlerService _settings, MessageManager _messageManager)
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

        [Command("set")]
        [Alias("s")]
        private async Task Set([Remainder]string RemainingText)
        {
            if (RemainingText.ToLower().Trim().StartsWith("auto manage") && IsAdmin(user, isDev))
            {
                RemainingText = RemoveString(RemainingText, "auto manage");

                if (RemainingText.ToLower().Trim().StartsWith("cat"))
                {
                    RemainingText = RemoveString(RemainingText, "cat");

                    AutoManageCatSet(RemainingText);
                }
                else if (RemainingText.ToLower().Trim().StartsWith("prefix"))
                {
                    RemainingText = RemoveString(RemainingText, "prefix");

                    AutoManagePrefixSet(RemainingText);
                }
            } 
            else if (RemainingText.ToLower().Trim().StartsWith("prefix") && IsAdmin(user, isDev))
            {
                RemainingText = RemoveString(RemainingText, "prefix");

                discordSettings.CustomPrefix = RemainingText;
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Set Custom Prefix to " + RemainingText, messageHandler);
            }

        }

        private void AutoManageCatSet(string RemainingText)
        {
            if (RemainingText.ToLower().Trim().StartsWith("limit") && IsAdmin(user, isDev))
            {
                RemainingText = RemoveString(RemainingText, "limit");

                if (Context.Guild.CategoryChannels.Any(x => RemainingText.ToLower().StartsWith(x.Name.ToLower())))
                {
                    var cat = Context.Guild.CategoryChannels.First(x => RemainingText.ToLower().StartsWith(x.Name.ToLower()));
                    RemainingText = RemoveString(RemainingText, cat.Name);
                    discordSettings.AutoChannelManage = SetLFGLimit(discordSettings.AutoChannelManage, RemainingText, "LFG  Catergory", cat.Id, MentionType: "#");
                }
            }

            else if (Context.Guild.CategoryChannels.Any(x => x.Name.ToLower() == RemainingText.ToLower()))
            {
                var cat = Context.Guild.CategoryChannels.First(x => x.Name.ToLower() == RemainingText.ToLower());
                discordSettings.AutoChannelManage = AddFromList(discordSettings.AutoChannelManage, "LFG  Catergory", cat.Id, MentionType: "#");
            }
        }

        private void AutoManagePrefixSet(string RemainingText)
        {
            if (RemainingText.ToLower().Trim().StartsWith("limit") && IsAdmin(user, isDev))
            {
                RemainingText = RemoveString(RemainingText, "limit");
                if (discordSettings.AutoChannelManage.Any(x => RemainingText.ToLower().StartsWith(x.Item2)))
                {
                    var voicePrefix = discordSettings.AutoChannelManage.First(x => RemainingText.ToLower().StartsWith(x.Item2));
                    RemainingText = RemoveString(RemainingText, voicePrefix.Item2);
                    discordSettings.AutoChannelManage = SetLFGLimit(discordSettings.AutoChannelManage, RemainingText, "LFG  Catergory", voicePrefix: voicePrefix.Item2, MentionType: "#");
                }
            }

            else { 
                discordSettings.AutoChannelManage = AddFromList(discordSettings.AutoChannelManage, "LFG  Catergory", voicePrefix: RemainingText, MentionType: "#");
            }
        }

        [Command("remove")]
        [Alias("r")]
        private async Task Remove([Remainder]string RemainingText)
        {
            if (RemainingText.ToLower().Trim().StartsWith("auto manage") && IsAdmin(user, isDev))
            {
                RemainingText = RemoveString(RemainingText, "auto manage");

                if (RemainingText.ToLower().Trim().StartsWith("cat"))
                {
                    RemainingText = RemoveString(RemainingText, "cat");
                    if (Context.Guild.CategoryChannels.Any(x => x.Name.ToLower() == RemainingText.ToLower()))
                    {
                        var cat = Context.Guild.CategoryChannels.First(x => x.Name.ToLower() == RemainingText.ToLower());
                        discordSettings.AutoChannelManage = RemoveFromList(discordSettings.AutoChannelManage, "LFG Catergory", cat.Id, MentionType: "#");
                    }
                    else
                        addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "No category with the name " + RemainingText, messageHandler);
                }
                else if (RemainingText.ToLower().Trim().StartsWith("prefix"))
                {
                    RemainingText = RemoveString(RemainingText, "prefix");

                    discordSettings.AutoChannelManage = RemoveFromList(discordSettings.AutoChannelManage, "LFG Catergory", voicePrefix: RemainingText);
                }
            }

        }

        [Command("add")]
        [Alias("a")]
        private async Task Add([Remainder]string RemainingText)
        {
        }

        [Command("create")]
        [Alias("c")]
        private async Task Create([Remainder]string RemainingText)
        {
        }



        public List<Tuple<ulong?, string, int>> SetLFGLimit(List<Tuple<ulong?, string, int>> list, string text, string type, ulong? catID = null, string voicePrefix = null, string MentionType = "@")
        {
            int a = 0;
            Int32.TryParse(text, out a);

            var item = GetVoiceSettings(list, user.VoiceChannel);

            if (a <= 99)
            {
                if (item.Item1 != null)
                {
                    
                    if (item != null)
                    {
                        list.Remove(item);
                        list.Add(new Tuple<ulong?, string, int>(catID, null, a));
                        addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Set <" + MentionType + catID + "> Defualt user size to " + a, messageHandler);
                    }
                }
                else if (item.Item2 != null)
                {
                    if (item != null)
                    {
                        list.Remove(item);
                        list.Add(new Tuple<ulong?, string, int>(null, voicePrefix, a));
                        addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Set " + voicePrefix +" prefix Defualt user size to " + a, messageHandler);
                    }
                }
            }
            else
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "To Large of a Number try Between 0 - 99 (0 will set it as open to everyone)", messageHandler);


            return list;
        }

        enum ManageType {nul, CatID, VoicePrefix};

        public List<Tuple<ulong?, string, int>> AddFromList(List<Tuple<ulong?, string , int>> list, string type, ulong? catID = null, string voicePrefix = null, string MentionType = "@")
        {

            var manageType = ManageType.nul;


            if (catID != null)
                manageType = ManageType.CatID;
            else if (voicePrefix != null)
                manageType = ManageType.VoicePrefix;

            if (manageType == ManageType.CatID)
                if (!list.Any(x => x.Item1 == catID))
                {
                    list.Add(new Tuple<ulong?, string, int>(catID, null, 4));
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Added <" + MentionType + catID + "> to " + type, messageHandler);
                }
                else
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Allready <" + MentionType + catID + "> in " + type, messageHandler);

            if (manageType == ManageType.VoicePrefix)
                if (!list.Any(x => x.Item2 == voicePrefix.ToLower()))
                {
                    list.Add(new Tuple<ulong?, string, int>(null, voicePrefix.ToLower(), 4));
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Added " + voicePrefix+ " prefix to " + type, messageHandler);
                }
                else
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Allready " + voicePrefix + " prefix in " + type, messageHandler);

            if(manageType == ManageType.nul)
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Looks Like you found a error in the code, DM Milky#0001", messageHandler);

            return list;
        }

        public List<Tuple<ulong?, string, int>> RemoveFromList(List<Tuple<ulong?, string, int>> list, string type, ulong? catID = 0, string voicePrefix = "", string MentionType = "@")
        {
            var item = GetVoiceSettings(list, user.VoiceChannel);

            if (item != null)
            {
                list.Remove(item);

                if (catID != null)
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Removed <" + MentionType + catID + "> from " + type, messageHandler);
                else if (voicePrefix != "")
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Removed " + voicePrefix + " prefix from " + type, messageHandler);
            }
            else
            {
                if (catID != null)
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "no <" + MentionType + catID + "> in " + type, messageHandler);
                else if (voicePrefix != "")
                    addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "no " + voicePrefix + " prefix in " + type, messageHandler);
            }


            return list;
        }

        public string setString(string text, string type)
        {
            addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Set " + type + " to:\n" + text, messageHandler);

            return text;
        }

        public List<ulong> AddFromList(List<ulong> list, ulong newUlong, string type, string MentionType = "@")
        {
            if (!list.Any(x => x == newUlong))
            {
                list.Add(newUlong);
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Added <" + MentionType + newUlong + "> to " + type, messageHandler);
            }
            else
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Allready <@" + MentionType + newUlong + "> in " + type, messageHandler);

            return list;
        }

        public List<ulong> RemoveFromList(List<ulong> list, ulong newUlong, string type, string MentionType = "@")
        {
            if (list.Any(x => x == newUlong))
            {
                list.Remove(newUlong);
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "Removed <" + MentionType + newUlong + "> to " + type, messageHandler);
            }
            else
                addtext(Context.Guild, Context.Message.Channel as SocketTextChannel, user, "No <@" + MentionType + newUlong + "> in " + type, messageHandler);

            return list;
        }
    }
}
