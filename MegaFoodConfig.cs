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
        public ConfigEntry<float> HealthReduction;
        public ConfigEntry<float> EitrReduction;
    }

    public static class MegaFoodConfig
    {
        public static FoodStats MegaYgg { get; private set; }
        public static FoodStats MegaEgg { get; private set; }
        public static FoodStats MegaJerk { get; private set; }
        public static MeadStats MegaMead { get; private set; }

        public static ConfigEntry<int> StackSize { get; private set; }

        public static void Bind(ConfigFile config)
        {
            MegaYgg = BindFoodSection(config, "MegaYgg");
            MegaEgg = BindFoodSection(config, "MegaEgg");
            MegaJerk = BindFoodSection(config, "MegaJerk");

            MegaMead = new MeadStats
            {
                StaminaRegen    = config.Bind("MegaMead", "StaminaRegeneration", 100f, "Percentage boost to stamina regeneration."),
                HealthRegen     = config.Bind("MegaMead", "HealthRegeneration",  100f, "Percentage boost to health regeneration."),
                EitrRegen       = config.Bind("MegaMead", "EitrRegeneration",    100f, "Percentage boost to eitr regeneration."),
                StaminaReduction = config.Bind("MegaMead", "StaminaReduction",   50f,  "Percentage reduction to stamina usage."),
                HealthReduction  = config.Bind("MegaMead", "HealthReduction",    50f,  "Percentage reduction to health drain."),
                EitrReduction    = config.Bind("MegaMead", "EitrReduction",      50f,  "Percentage reduction to eitr usage."),
            };

            StackSize = config.Bind("Global", "StackSize", 100, "Max stack size for all MegaFood items.");
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
