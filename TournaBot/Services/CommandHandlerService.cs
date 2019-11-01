using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace BehaveBot.Services
{
    using static MessageManager;
    using static BehaveBot.Classes.DiscordExtensions;

    public class CommandHandlerService
    {
        public DiscordSocketClient discord;
        private CommandService commands;
        private IServiceProvider provider;
        private SettingsHandlerService settings;
        private Dictionary<ulong, Dictionary<ulong, channelMesages>> messageHandler;

        public async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
        }

        public CommandHandlerService(IServiceProvider _provider, DiscordSocketClient _discord, CommandService _commands, SettingsHandlerService _settings, MessageManager _messageManager)
        {
            discord = _discord;
            commands = _commands;
            provider = _provider;
            settings = _settings;
            messageHandler = _messageManager.MessageHandler;

            discord.MessageReceived += HandleCommandAsync;

            discord.UserVoiceStateUpdated += Discord_UserVoiceStateUpdated;

            discord.SetGameAsync("");
        }

        private async Task Discord_UserVoiceStateUpdated(SocketUser _user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var user = _user as SocketGuildUser;
            if (user == null)
                return;

            var guildsettings = GetGuildSettings(user.Guild.Id);

            if (oldState.VoiceChannel != null)
            {
                var autoManageChannel = GetVoiceSettings(guildsettings.AutoChannelManage, oldState.VoiceChannel);

                if (autoManageChannel != null && oldState.VoiceChannel.Users.Count() == 0)
                {
                    await ResetVoiceChannel(oldState.VoiceChannel, autoManageChannel.Item3);
                }
            }
        }

        private async Task ResetVoiceChannel(SocketVoiceChannel chnl, int userCount)
        {
            await chnl.ModifyAsync(x => x.UserLimit = userCount);

            await chnl.SyncPermissionsAsync();
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (socketMessage.Author.IsBot)
                return;

            var DefaultSettings = settings.discord.defaultDiscordSettings;//get defualt discord settings

            var msg = socketMessage as SocketUserMessage;
            if (msg == null)
                return;

            var context = new SocketCommandContext(discord, msg);
            int argPos = 0;
            var botPrefix = discord.CurrentUser as SocketSelfUser;

            bool canUseBotPrefix = DefaultSettings.allowBotTagPrefix;

            if (msg.Channel is IDMChannel)//Dms of the user
            {

            }
            else if (msg.Channel is SocketGuildChannel chnl)//guild channel
            {
                var guildsettings = GetGuildSettings(chnl.Guild.Id);

                var prefix = guildsettings.CustomPrefix;
                if (prefix == "")
                    prefix = DefaultSettings.defaultPrefix;

                if (msg.HasStringPrefix(prefix, ref argPos) || (canUseBotPrefix && msg.HasMentionPrefix(botPrefix, ref argPos)))
                {
                    await commands.ExecuteAsync(context, argPos, provider);
                }
            }
        }

        private DiscordSettings.CustomDiscordSetting GetGuildSettings(ulong guildId)
        {

            var guildsettings = new DiscordSettings.CustomDiscordSetting();

            if (!settings.discord.customDiscordSettings.TryGetValue(guildId, out guildsettings))//check for guild settings
            {//if failed to find settings add creat them
                guildsettings = new DiscordSettings.CustomDiscordSetting { DiscordID = guildId };

                settings.discord.customDiscordSettings.Add(guildId, guildsettings);
            }

            return guildsettings;
        }


    }
}