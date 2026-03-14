using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;

namespace MegaFood
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikal.megafood";
        public const string PluginName = "MegaFood";
        public const string PluginVersion = "1.1.1";

        private static ManualLogSource _logger;
        private readonly Harmony _harmony = new Harmony(PluginGUID);
        private static FileSystemWatcher _configWatcher;
        private static BepInEx.Configuration.ConfigFile _config;

        private void Awake()
        {
            _logger = Logger;
            _config = Config;

            MegaFoodConfig.Bind(Config);
            SetupConfigWatcher();
            _harmony.PatchAll();

            _logger.LogInfo($"{PluginName} v{PluginVersion} loaded!");
            _logger.LogInfo($"Live config reloading enabled - edit {Config.ConfigFilePath} and save to apply changes!");
        }

        private void SetupConfigWatcher()
        {
            string configPath = Path.GetDirectoryName(Config.ConfigFilePath);
            string configFile = Path.GetFileName(Config.ConfigFilePath);

            _configWatcher = new FileSystemWatcher(configPath, configFile);
            _configWatcher.Changed += OnConfigChanged;
            _configWatcher.Created += OnConfigChanged;
            _configWatcher.Renamed += OnConfigChanged;
            _configWatcher.IncludeSubdirectories = false;
            _configWatcher.SynchronizingObject = null;
            _configWatcher.EnableRaisingEvents = true;

            _logger.LogInfo($"Config watcher started for: {configFile}");
        }

        private static void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                System.Threading.Thread.Sleep(100);
                _config.Reload();
                _logger.LogInfo("Config reloaded! Changes applied.");

                if (Player.m_localPlayer != null)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "MegaFood Config Reloaded!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to reload config: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            if (_configWatcher != null)
            {
                _configWatcher.EnableRaisingEvents = false;
                _configWatcher.Dispose();
                _configWatcher = null;
            }
            _harmony.UnpatchSelf();
        }
    }
}
