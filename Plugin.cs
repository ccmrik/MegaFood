using BepInEx;
using HarmonyLib;

namespace MegaFood
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikal.megafood";
        public const string PluginName = "MegaFood";
        public const string PluginVersion = "1.0.2";

        private readonly Harmony _harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            MegaFoodConfig.Bind(Config);
            _harmony.PatchAll();
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
