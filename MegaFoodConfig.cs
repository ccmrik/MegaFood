using BepInEx.Configuration;

namespace MegaFood
{
    public class FoodStats
    {
        public ConfigEntry<float> Health;
        public ConfigEntry<float> Stamina;
        public ConfigEntry<float> Eitr;
    }

    public static class MegaFoodConfig
    {
        public static FoodStats MegaYgg { get; private set; }
        public static FoodStats MegaEgg { get; private set; }
        public static FoodStats MegaJerk { get; private set; }

        public static void Bind(ConfigFile config)
        {
            MegaYgg = BindSection(config, "MegaYgg");
            MegaEgg = BindSection(config, "MegaEgg");
            MegaJerk = BindSection(config, "MegaJerk");
        }

        private static FoodStats BindSection(ConfigFile config, string section)
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
