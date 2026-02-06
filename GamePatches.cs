using HarmonyLib;

namespace MegaFood
{
    [HarmonyPatch(typeof(ObjectDB), "Awake")]
    public static class ObjectDBAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ObjectDB __instance)
        {
            MegaFoodItems.RegisterAll(__instance);
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
    public static class ObjectDBCopyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ObjectDB __instance)
        {
            MegaFoodItems.RegisterAll(__instance);
        }
    }

    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    public static class ZNetSceneAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ZNetScene __instance)
        {
            MegaFoodItems.RegisterPrefabs(__instance);
        }
    }

    [HarmonyPatch(typeof(Localization), nameof(Localization.SetupLanguage))]
    public static class LocalizationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Localization __instance)
        {
            AddTranslation(__instance, "item_megaygg", "MegaYgg");
            AddTranslation(__instance, "item_megaygg_desc",
                "A powerfully enhanced Yggdrasil porridge infused with surtling energy. Provides extraordinary nourishment.");

            AddTranslation(__instance, "item_megaegg", "MegaEgg");
            AddTranslation(__instance, "item_megaegg_desc",
                "An egg supercharged with surtling essence. Its purple glow hints at immense power.");

            AddTranslation(__instance, "item_megajerk", "MegaJerk");
            AddTranslation(__instance, "item_megajerk_desc",
                "Boar jerky imbued with surtling fire. The purple sheen marks it as food fit for legends.");
        }

        private static void AddTranslation(Localization loc, string key, string value)
        {
            loc.m_translations[key] = value;
        }
    }
}
