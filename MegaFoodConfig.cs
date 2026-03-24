using BepInEx.Configuration;

namespace MegaFood
{
    public static class MegaFoodConfig
    {
        // 1. General
        public static ConfigEntry<int> StackSize { get; private set; }
        public static ConfigEntry<int> Duration { get; private set; }

        // 2. Food Items
        public static ConfigEntry<bool> EnableMegaJerk { get; private set; }
        public static ConfigEntry<bool> EnableMegaEgg { get; private set; }
        public static ConfigEntry<bool> EnableMegaYgg { get; private set; }
        public static ConfigEntry<bool> EnableMegaMead { get; private set; }

        // 3. MegaMead (effect toggles — named after vanilla mead equivalents)
        public static ConfigEntry<bool> FireResistance { get; private set; }
        public static ConfigEntry<bool> FrostResistance { get; private set; }
        public static ConfigEntry<bool> PoisonResistance { get; private set; }
        public static ConfigEntry<bool> LingeringHealing { get; private set; }
        public static ConfigEntry<bool> LingeringStamina { get; private set; }
        public static ConfigEntry<bool> LingeringEitr { get; private set; }
        public static ConfigEntry<bool> LovePotion { get; private set; }
        public static ConfigEntry<bool> Berserkir { get; private set; }
        public static ConfigEntry<bool> AntiSting { get; private set; }
        public static ConfigEntry<bool> DraughtOfVananidir { get; private set; }
        public static ConfigEntry<bool> TonicOfRatatosk { get; private set; }
        public static ConfigEntry<bool> TrollEndurance { get; private set; }
        public static ConfigEntry<bool> AnimalWhispers { get; private set; }
        public static ConfigEntry<bool> Lightfoot { get; private set; }

        // 4. Debug
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public static void Bind(ConfigFile config)
        {
            StackSize = config.Bind("1. General", "StackSize", 100, "Max stack size for all MegaFood items.");
            Duration  = config.Bind("1. General", "Duration",  60,  "Duration in minutes for all food items and MegaMead status effect.");

            EnableMegaJerk = config.Bind("2. Food Items", "EnableMegaJerk", true, "Enable MegaJerk food item (150 Health/Stamina/Eitr).");
            EnableMegaEgg  = config.Bind("2. Food Items", "EnableMegaEgg",  true, "Enable MegaEgg food item (150 Health/Stamina/Eitr).");
            EnableMegaYgg  = config.Bind("2. Food Items", "EnableMegaYgg",  true, "Enable MegaYgg food item (150 Health/Stamina/Eitr).");
            EnableMegaMead = config.Bind("2. Food Items", "EnableMegaMead", true, "Enable MegaMead brewing (base + fermented mead with combined effects).");

            FireResistance      = config.Bind("3. MegaMead", "FireResistance",      true, "Fire Resistance Barley Wine — Very high fire resistance.");
            FrostResistance     = config.Bind("3. MegaMead", "FrostResistance",     true, "Frost Resistance Mead — Very high frost resistance.");
            PoisonResistance    = config.Bind("3. MegaMead", "PoisonResistance",    true, "Poison Resistance Mead — Very high poison resistance.");
            LingeringHealing    = config.Bind("3. MegaMead", "LingeringHealing",    true, "Lingering Healing Mead — +25% health regeneration.");
            LingeringStamina    = config.Bind("3. MegaMead", "LingeringStamina",    true, "Lingering Stamina Mead — +25% stamina regeneration.");
            LingeringEitr       = config.Bind("3. MegaMead", "LingeringEitr",       true, "Lingering Eitr Mead — +25% eitr regeneration.");
            LovePotion          = config.Bind("3. MegaMead", "LovePotion",          true, "Love Potion — Troll pheromones (trolls flee).");
            Berserkir           = config.Bind("3. MegaMead", "Berserkir",           true, "Berserkir Mead — Attack, block and dodge stamina use -80%.");
            AntiSting           = config.Bind("3. MegaMead", "AntiSting",           true, "Anti-Sting Concoction — Prevent Deathsquito attacks (stealth + silent).");
            DraughtOfVananidir  = config.Bind("3. MegaMead", "DraughtOfVananidir",  true, "Draught of Vananidir — -50% swimming stamina consumption.");
            TonicOfRatatosk     = config.Bind("3. MegaMead", "TonicOfRatatosk",     true, "Tonic of Ratatosk — +15% walking/running speed, +7.5% swimming speed.");
            TrollEndurance      = config.Bind("3. MegaMead", "TrollEndurance",      true, "Mead of Troll Endurance — +250 carry weight.");
            AnimalWhispers      = config.Bind("3. MegaMead", "AnimalWhispers",      true, "Brew of Animal Whispers — x2 taming speed.");
            Lightfoot           = config.Bind("3. MegaMead", "Lightfoot",           true, "Lightfoot Mead — -30% jump stamina cost, +20% jump height.");

            DebugMode = config.Bind("4. Debug", "DebugMode", false,
                "Enable verbose debug logging to BepInEx console/log");
        }
    }
}
