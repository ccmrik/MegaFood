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
        public const string PluginVersion = "1.1.7";

        private static ManualLogSource _logger;
        private readonly Harmony _harmony = new Harmony(PluginGUID);
        private static FileSystemWatcher _configWatcher;
        private static BepInEx.Configuration.ConfigFile _config;

        private void Awake()
        {
            _logger = Logger;
            _config = Config;

            MigrateConfig(Config.ConfigFilePath);
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

        private static void MigrateConfig(string configPath)
        {
            try
            {
                if (!File.Exists(configPath)) return;
                string text = File.ReadAllText(configPath);
                bool changed = false;

                changed |= MigrateCfgSection(ref text, "Global", "1. Global");
                changed |= MigrateCfgSection(ref text, "MegaYgg", "2. MegaYgg");
                changed |= MigrateCfgSection(ref text, "MegaEgg", "3. MegaEgg");
                changed |= MigrateCfgSection(ref text, "MegaJerk", "4. MegaJerk");
                changed |= MigrateCfgSection(ref text, "MegaMead", "5. MegaMead");
                changed |= MigrateCfgSection(ref text, "MegaMead Effects", "6. MegaMead Effects");
                changed |= MigrateCfgSection(ref text, "Debug", "7. Debug");

                if (changed)
                    File.WriteAllText(configPath, text.TrimEnd() + "\n");
            }
            catch { }
        }

        private static bool MigrateCfgSection(ref string text, string oldName, string newName)
        {
            string oldHeader = "[" + oldName + "]";
            int idx = text.IndexOf(oldHeader, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return false;

            int sectionEnd = text.IndexOf("\n[", idx + oldHeader.Length, StringComparison.Ordinal);

            if (newName == null || text.IndexOf("[" + newName + "]", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (sectionEnd < 0)
                    text = text.Substring(0, idx).TrimEnd('\r', '\n');
                else
                    text = text.Substring(0, idx) + text.Substring(sectionEnd + 1);
            }
            else
            {
                text = text.Remove(idx, oldHeader.Length).Insert(idx, "[" + newName + "]");
            }
            return true;
        }
    }
}
