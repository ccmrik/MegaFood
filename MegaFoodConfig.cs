using BepInEx.Configuration;

namespace MegaFood
{
    public class MeadStats
    {
        public ConfigEntry<float> StaminaRegen;
        public ConfigEntry<float> HealthRegen;
        public ConfigEntry<float> EitrRegen;
        public ConfigEntry<float> StaminaReduction;
        public ConfigEntry<float> EitrReduction;
        public ConfigEntry<float> SpeedIncrease;

        // Integrated mead effect toggles
        public ConfigEntry<bool> EnableBugRepellent;
        public ConfigEntry<bool> EnableSwimmer;
        public ConfigEntry<bool> EnableLightfoot;
        public ConfigEntry<bool> EnableTamer;
        public ConfigEntry<bool> EnableTrollPheromones;
    }

    public static class MegaFoodConfig
    {
        public static ConfigEntry<bool> EnableMegaJerk { get; private set; }
        public static ConfigEntry<bool> EnableMegaEgg { get; private set; }
        public static ConfigEntry<bool> EnableMegaYgg { get; private set; }
        public static ConfigEntry<bool> EnableMegaMead { get; private set; }
        public static MeadStats MegaMead { get; private set; }

        public static ConfigEntry<int> StackSize { get; private set; }
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public static void Bind(ConfigFile config)
        {
            StackSize = config.Bind("1. General", "StackSize", 100, "Max stack size for all MegaFood items.");

            EnableMegaJerk = config.Bind("2. Food Items", "EnableMegaJerk", true, "Enable MegaJerk food item (150 Health/Stamina/Eitr, 1hr duration).");
            EnableMegaEgg  = config.Bind("2. Food Items", "EnableMegaEgg",  true, "Enable MegaEgg food item (150 Health/Stamina/Eitr, 1hr duration).");
            EnableMegaYgg  = config.Bind("2. Food Items", "EnableMegaYgg",  true, "Enable MegaYgg food item (150 Health/Stamina/Eitr, 1hr duration).");
            EnableMegaMead = config.Bind("2. Food Items", "EnableMegaMead", true, "Enable MegaMead brewing (base + fermented mead with status effects).");

            MegaMead = new MeadStats
            {
                StaminaRegen    = config.Bind("3. MegaMead", "StaminaRegeneration", 100f, "Percentage boost to stamina regeneration."),
                HealthRegen     = config.Bind("3. MegaMead", "HealthRegeneration",  100f, "Percentage boost to health regeneration."),
                EitrRegen       = config.Bind("3. MegaMead", "EitrRegeneration",    100f, "Percentage boost to eitr regeneration."),
                StaminaReduction = config.Bind("3. MegaMead", "StaminaReduction",   50f,  "Percentage reduction to stamina usage."),
                EitrReduction    = config.Bind("3. MegaMead", "EitrReduction",      50f,  "Percentage reduction to eitr usage."),
                SpeedIncrease    = config.Bind("3. MegaMead", "SpeedIncrease",      15f,  "Percentage increase to movement speed."),

                EnableBugRepellent    = config.Bind("4. MegaMead Effects", "BugRepellent",    true, "Grants bug repellent effect (stealth + silent, prevents insect aggro)."),
                EnableSwimmer         = config.Bind("4. MegaMead Effects", "Swimmer",         true, "Grants swimmer effect (faster swimming, reduced swim stamina)."),
                EnableLightfoot       = config.Bind("4. MegaMead Effects", "Lightfoot",       true, "Grants lightfoot effect (no fall damage)."),
                EnableTamer           = config.Bind("4. MegaMead Effects", "Tamer",           true, "Grants tamer effect (2x taming speed)."),
                EnableTrollPheromones = config.Bind("4. MegaMead Effects", "TrollPheromones", true, "Grants troll pheromone effect (trolls flee)."),
            };

            DebugMode = config.Bind("5. Debug", "DebugMode", false,
                "Enable verbose debug logging to BepInEx console/log");
        }
    }
}
