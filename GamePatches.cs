using HarmonyLib;
using UnityEngine;

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
                "A powerfully enhanced Yggdrasil porridge infused with surtling energy.\n+150 Health, Stamina & Eitr.");

            AddTranslation(__instance, "item_megaegg", "MegaEgg");
            AddTranslation(__instance, "item_megaegg_desc",
                "An egg supercharged with surtling essence. Its purple glow hints at immense power.\n+150 Health, Stamina & Eitr.");

            AddTranslation(__instance, "item_megajerk", "MegaJerk");
            AddTranslation(__instance, "item_megajerk_desc",
                "Boar jerky imbued with surtling fire. The purple sheen marks it as food fit for legends.\n+150 Health, Stamina & Eitr.");

            AddTranslation(__instance, "item_megameadbase", "MegaMead Base");
            AddTranslation(__instance, "item_megameadbase_desc",
                "A volatile mead base crackling with surtling energy. Place in a Fermenter.");

            AddTranslation(__instance, "item_megamead", "MegaMead");
            AddTranslation(__instance, "item_megamead_desc",
                "A legendary mead combining the power of 14 different brews into one. The purple shimmer is mesmerising.");

            AddTranslation(__instance, "se_megamead", "MegaMead");
            AddTranslation(__instance, "se_megamead_tooltip",
                "Fire, Frost & Poison resistance (very high)\n" +
                "+25% Health, Stamina & Eitr regeneration\n" +
                "Attack, Block & Dodge stamina use -80%\n" +
                "Prevents Deathsquito attacks\n" +
                "-50% swimming stamina consumption\n" +
                "+15% walk/run speed, +7.5% swim speed\n" +
                "+250 carry weight\n" +
                "x2 taming speed\n" +
                "-30% jump stamina, +20% jump height\n" +
                "Troll pheromones");
        }

        private static void AddTranslation(Localization loc, string key, string value)
        {
            loc.m_translations[key] = value;
        }
    }

    // Brew of Animal Whispers (MeadTamer) — x2 taming speed
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.DecreaseRemainingTime))]
    public static class TameablePatch
    {
        private static readonly int MegaMeadHash = "SE_MegaMead".GetStableHashCode();

        [HarmonyPrefix]
        public static void Prefix(ref float time)
        {
            if (!MegaFoodConfig.AnimalWhispers.Value) return;

            var player = Player.m_localPlayer;
            if (player != null && player.m_seman != null &&
                player.m_seman.HaveStatusEffect(MegaMeadHash))
            {
                time *= 2f;
            }
        }
    }

    // Lightfoot Mead (MeadLightfoot) — +20% jump height
    [HarmonyPatch(typeof(Character), nameof(Character.Jump))]
    public static class JumpHeightPatch
    {
        private static readonly int MegaMeadHash = "SE_MegaMead".GetStableHashCode();

        [HarmonyPrefix]
        public static void Prefix(Character __instance, out float __state)
        {
            __state = __instance.m_jumpForce;
            if (__instance is Player player &&
                MegaFoodConfig.Lightfoot.Value &&
                player.m_seman?.HaveStatusEffect(MegaMeadHash) == true)
            {
                __instance.m_jumpForce *= 1.2f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Character __instance, float __state)
        {
            __instance.m_jumpForce = __state;
        }
    }

    // Berserkir Mead (MeadBzerker) — -80% dodge stamina
    [HarmonyPatch(typeof(Player), nameof(Player.Dodge))]
    public static class DodgeStaminaPatch
    {
        private static readonly int MegaMeadHash = "SE_MegaMead".GetStableHashCode();

        [HarmonyPrefix]
        public static void Prefix(Player __instance, out float __state)
        {
            __state = __instance.m_dodgeStaminaUsage;
            if (MegaFoodConfig.Berserkir.Value &&
                __instance.m_seman?.HaveStatusEffect(MegaMeadHash) == true)
            {
                __instance.m_dodgeStaminaUsage *= 0.2f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Player __instance, float __state)
        {
            __instance.m_dodgeStaminaUsage = __state;
        }
    }
}
