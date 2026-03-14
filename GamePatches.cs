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

    [HarmonyPatch(typeof(Fermenter), "Awake")]
    public static class FermenterAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Fermenter __instance)
        {
            MegaFoodItems.RegisterFermenterConversion(__instance);
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

            AddTranslation(__instance, "item_megameadbase", "MegaMead Base");
            AddTranslation(__instance, "item_megameadbase_desc",
                "A volatile mead base crackling with surtling energy. Place in a Fermenter.");

            AddTranslation(__instance, "item_megamead", "MegaMead");
            AddTranslation(__instance, "item_megamead_desc",
                "A legendary mead that supercharges regeneration while reducing resource costs. The purple shimmer is mesmerising.");

            AddTranslation(__instance, "se_megamead", "MegaMead");
            AddTranslation(__instance, "se_megamead_tooltip",
                "Stamina, Health & Eitr regeneration greatly increased.\nStamina & Eitr usage reduced.\nAll elemental & physical damage resistance.\nMovement speed increased.");
        }

        private static void AddTranslation(Localization loc, string key, string value)
        {
            loc.m_translations[key] = value;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UseEitr))]
    public static class UseEitrPatch
    {
        private static readonly int MegaMeadHash = "SE_MegaMead".GetStableHashCode();

        [HarmonyPrefix]
        public static void Prefix(Player __instance, ref float v)
        {
            if (v > 0f && __instance.m_seman != null &&
                __instance.m_seman.HaveStatusEffect(MegaMeadHash))
            {
                v *= 1f - MegaFoodConfig.MegaMead.EitrReduction.Value / 100f;
            }
        }
    }
}
