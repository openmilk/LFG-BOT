using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using static BehaveBot.Services.DiscordSettings;
using static BehaveBot.Services.DiscordSettings.CustomDiscordSetting;

namespace BehaveBot.Services
{
    public class DiscordSettings
    {

        public DefualtDiscordSettings defaultDiscordSettings { get; set; } = new DefualtDiscordSettings ();

        public class DefualtDiscordSettings
        {
            public string botToken { get; set; } = "";
            public bool allowBotTagPrefix { get; set; } = true;
            public string defaultPrefix { get; set; } = "!";
            public List<ulong> botDevs { get; set; } = new List<ulong>();
        }


        public Dictionary<ulong, CustomDiscordSetting> customDiscordSettings { get; set; } = new Dictionary<ulong, CustomDiscordSetting>();

        public class CustomDiscordSetting
        {
            public ulong DiscordID { get; set; } = 0;// used so i dont have to do conversion on bot boot for every server

            public string CustomPrefix { get; set; } = "";

            public List<Tuple<ulong?, string, int>> AutoChannelManage { get; set; } = new List<Tuple<ulong?, string, int>>();//cat id, voice channel prefix,  defualt limit// either catif or voice channel prefix will be null
        }    
    }



    public class SettingsHandlerService
    {
        public DiscordSettings discord = new DiscordSettings(); //sets it so it is not null
        private Timer aTimer;

        
        private string guildsDirectory = Environment.CurrentDirectory + "\\Guilds";

        public SettingsHandlerService()
        {
            CreateFolder(guildsDirectory);

            LoadSettings();

            aTimer = new System.Timers.Timer(30000); //files updates every 30 seconds
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += SaveSettings;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void LoadSettings()
        {
            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\globalSettings.json"))
            {
                var loadedString = System.IO.File.ReadAllText(Environment.CurrentDirectory + "\\globalSettings.json");
                discord.defaultDiscordSettings = JsonConvert.DeserializeObject<DiscordSettings.DefualtDiscordSettings>(loadedString);
            }

            var GuildSettingsList = System.IO.Directory.GetFiles(guildsDirectory);

            foreach(var a in GuildSettingsList)//load all Custom Discord Settings
            {
                if (a.EndsWith(".json"))//make sore the file is a json
                {
                    var loadedString = System.IO.File.ReadAllText(a);

                    var customDiscordSettings = JsonConvert.DeserializeObject<CustomDiscordSetting>(loadedString); 

                    //might be an issue with discord ids allready existing and crashing the bot, key word "might" 
                    discord.customDiscordSettings.Add(customDiscordSettings.DiscordID, customDiscordSettings);
                }
            }
        }

        /*public void ReloadSettings() //kinda obsolete with SaveSettings, maybe there will be a use for it in the future?
        {
            LoadSettings();
        }
        */

        public void SaveSettings(object source, ElapsedEventArgs e) //save all the settings
        {
            aTimer.Enabled = false;//turn timer off so it wont repeat while saving
            Console.WriteLine("Saving settings " + DateTime.Now);

            CreateFolder(guildsDirectory); //make sure folder was not deleted during runtime

            foreach (var a in discord.customDiscordSettings)
            {
                var Json = JsonConvert.SerializeObject(a.Value, Formatting.Indented);

                try//try as saving a file sometimes can cause issues, never found the issue so i wraped it in a try loop
                {
                    System.IO.File.WriteAllText(guildsDirectory + "\\" + a.Key + ".json", Json);
                }
                catch (Exception)
                {
                    Console.WriteLine("error 1//");
                }
            }


            //save default settings
            {//just so i dont have hirachy problems with jsondata
                var jsonData = JsonConvert.SerializeObject(discord.defaultDiscordSettings, Formatting.Indented);

                try//try as saving a file sometimes can cause issues, never found the issue so i wraped it in a try loop
                {
                    System.IO.File.WriteAllText(Environment.CurrentDirectory + "\\globalSettings.json", jsonData);
                }
                catch (Exception)
                {
                    Console.WriteLine("error 2//");
                }
            }

            aTimer.Enabled = true;//turn it back on so it can save
        }

        public void CreateFolder(string directory)
        {
            System.IO.Directory.CreateDirectory(directory);
        }
    }
}