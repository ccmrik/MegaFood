using BepInEx.Configuration;

namespace MegaFood
{
    public class FoodStats
    {
        public ConfigEntry<float> Health;
        public ConfigEntry<float> Stamina;
        public ConfigEntry<float> Eitr;
    }

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
        public static FoodStats MegaYgg { get; private set; }
        public static FoodStats MegaEgg { get; private set; }
        public static FoodStats MegaJerk { get; private set; }
        public static MeadStats MegaMead { get; private set; }

        public static ConfigEntry<int> StackSize { get; private set; }
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public static void Bind(ConfigFile config)
        {
            MegaYgg = BindFoodSection(config, "2. MegaYgg");
            MegaEgg = BindFoodSection(config, "3. MegaEgg");
            MegaJerk = BindFoodSection(config, "4. MegaJerk");

            MegaMead = new MeadStats
            {
                StaminaRegen    = config.Bind("5. MegaMead", "StaminaRegeneration", 100f, "Percentage boost to stamina regeneration."),
                HealthRegen     = config.Bind("5. MegaMead", "HealthRegeneration",  100f, "Percentage boost to health regeneration."),
                EitrRegen       = config.Bind("5. MegaMead", "EitrRegeneration",    100f, "Percentage boost to eitr regeneration."),
                StaminaReduction = config.Bind("5. MegaMead", "StaminaReduction",   50f,  "Percentage reduction to stamina usage."),
                EitrReduction    = config.Bind("5. MegaMead", "EitrReduction",      50f,  "Percentage reduction to eitr usage."),
                SpeedIncrease    = config.Bind("5. MegaMead", "SpeedIncrease",      15f,  "Percentage increase to movement speed."),

                EnableBugRepellent    = config.Bind("6. MegaMead Effects", "BugRepellent",    true, "Grants bug repellent effect (stealth + silent, prevents insect aggro)."),
                EnableSwimmer         = config.Bind("6. MegaMead Effects", "Swimmer",         true, "Grants swimmer effect (faster swimming, reduced swim stamina)."),
                EnableLightfoot       = config.Bind("6. MegaMead Effects", "Lightfoot",       true, "Grants lightfoot effect (no fall damage)."),
                EnableTamer           = config.Bind("6. MegaMead Effects", "Tamer",           true, "Grants tamer effect (2x taming speed)."),
                EnableTrollPheromones = config.Bind("6. MegaMead Effects", "TrollPheromones", true, "Grants troll pheromone effect (trolls flee)."),
            };

            StackSize = config.Bind("1. Global", "StackSize", 100, "Max stack size for all MegaFood items.");

            DebugMode = config.Bind("7. Debug", "DebugMode", false,
                "Enable verbose debug logging to BepInEx console/log");
        }

        private static FoodStats BindFoodSection(ConfigFile config, string section)
        {
            return new FoodStats
            {
                Health  = config.Bind(section, "Health",  150f, "Health granted by this food."),
                Stamina = config.Bind(section, "Stamina", 150f, "Stamina granted by this food."),
                Eitr    = config.Bind(section, "Eitr",    150f, "Eitr granted by this food."),
            };
        }
    }
}
