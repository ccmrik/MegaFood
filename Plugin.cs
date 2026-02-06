using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MegaFood
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikal.megafood";
        public const string PluginName = "MegaFood";
        public const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log;

        private readonly Harmony _harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            Log = Logger;
            MegaFoodConfig.Bind(Config);
            _harmony.PatchAll();
            Log.LogInfo($"{PluginName} v{PluginVersion} loaded!");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
