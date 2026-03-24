using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MegaFood
{
    public static class MegaFoodItems
    {
        private static readonly Color PurpleTint = new Color(0.75f, 0.5f, 1.0f);

        private static readonly List<GameObject> CachedPrefabs = new List<GameObject>();
        private static readonly List<Recipe> CachedRecipes = new List<Recipe>();
        private static bool _prefabsCreated;

        // Inactive container prevents ZNetView.Awake from firing on clones
        private static GameObject _prefabContainer;

        // Exposed for the Fermenter patch
        public static GameObject MegaMeadBasePrefab { get; private set; }
        public static GameObject MegaMeadPrefab { get; private set; }

        public static void RegisterAll(ObjectDB objectDB)
        {
            if (objectDB.m_items.Count == 0 || objectDB.GetItemPrefab("Wood") == null)
                return;

            if (!_prefabsCreated)
            {
                InitContainer();

                if (MegaFoodConfig.EnableMegaYgg.Value)
                    CreateFood(objectDB, "YggdrasilPorridge", "MegaYgg",
                        "$item_megaygg", "$item_megaygg_desc");
                if (MegaFoodConfig.EnableMegaEgg.Value)
                    CreateFood(objectDB, "CookedEgg", "MegaEgg",
                        "$item_megaegg", "$item_megaegg_desc");
                if (MegaFoodConfig.EnableMegaJerk.Value)
                    CreateFood(objectDB, "BoarJerky", "MegaJerk",
                        "$item_megajerk", "$item_megajerk_desc");

                if (MegaFoodConfig.EnableMegaMead.Value)
                    CreateMead(objectDB);
                _prefabsCreated = true;
            }

            // (Re-)inject into ObjectDB � CopyOtherDB wipes the lists each time
            int added = 0;
            foreach (var prefab in CachedPrefabs)
            {
                if (objectDB.GetItemPrefab(prefab.name) == null)
                {
                    objectDB.m_items.Add(prefab);
                    objectDB.m_itemByHash[prefab.name.GetStableHashCode()] = prefab;
                    added++;
                }
            }

            if (added > 0)
                objectDB.UpdateRegisters();

            // Build recipes once (after items are in ObjectDB), then re-inject each call
            if (CachedRecipes.Count == 0)
                BuildRecipes(objectDB);

            foreach (var recipe in CachedRecipes)
            {
                if (!objectDB.m_recipes.Contains(recipe))
                    objectDB.m_recipes.Add(recipe);
            }
        }

        public static void RegisterPrefabs(ZNetScene zNetScene)
        {
            foreach (var prefab in CachedPrefabs)
            {
                if (prefab == null) continue;
                int hash = prefab.name.GetStableHashCode();

                if (!zNetScene.m_namedPrefabs.ContainsKey(hash))
                {
                    zNetScene.m_prefabs.Add(prefab);
                    zNetScene.m_namedPrefabs[hash] = prefab;
                }
            }
        }

        public static void RegisterFermenterConversion(Fermenter fermenter)
        {
            if (MegaMeadBasePrefab == null || MegaMeadPrefab == null) return;

            var baseItem = MegaMeadBasePrefab.GetComponent<ItemDrop>();
            if (fermenter.m_conversion.Any(c => c.m_from == baseItem))
                return;

            fermenter.m_conversion.Add(new Fermenter.ItemConversion
            {
                m_from = baseItem,
                m_to = MegaMeadPrefab.GetComponent<ItemDrop>(),
                m_producedItems = 6,
            });
        }

        #region Prefab creation

        private static void InitContainer()
        {
            _prefabContainer = new GameObject("MegaFood_Prefabs");
            _prefabContainer.SetActive(false);
            Object.DontDestroyOnLoad(_prefabContainer);
        }

        private static void CreateFood(ObjectDB objectDB, string basePrefab,
            string newName, string nameToken, string descToken)
        {
            var clone = ClonePrefab(objectDB, basePrefab, newName);
            if (clone == null) return;

            var shared = clone.GetComponent<ItemDrop>().m_itemData.m_shared;
            shared.m_name = nameToken;
            shared.m_description = descToken;
            ApplyMegaStats(shared);
            ApplyCommonItemProps(shared);
            ApplyPurpleTint(clone);

            CachedPrefabs.Add(clone);
        }

        private static void CreateMead(ObjectDB objectDB)
        {
            // MegaMead Base � craftable at cauldron, goes into fermenter
            MegaMeadBasePrefab = ClonePrefab(objectDB, "MeadBaseEitrMinor", "MegaMeadBase");
            if (MegaMeadBasePrefab != null)
            {
                var baseShared = MegaMeadBasePrefab.GetComponent<ItemDrop>().m_itemData.m_shared;
                baseShared.m_name = "$item_megameadbase";
                baseShared.m_description = "$item_megameadbase_desc";
                ApplyCommonItemProps(baseShared);
                ApplyPurpleTint(MegaMeadBasePrefab);
                CachedPrefabs.Add(MegaMeadBasePrefab);
            }

            // MegaMead � produced by fermenter, drinkable
            MegaMeadPrefab = ClonePrefab(objectDB, "MeadEitrMinor", "MegaMead");
            if (MegaMeadPrefab != null)
            {
                var meadShared = MegaMeadPrefab.GetComponent<ItemDrop>().m_itemData.m_shared;
                meadShared.m_name = "$item_megamead";
                meadShared.m_description = "$item_megamead_desc";
                ApplyCommonItemProps(meadShared);
                ApplyPurpleTint(MegaMeadPrefab);
                ApplyMeadStatusEffect(meadShared);
                CachedPrefabs.Add(MegaMeadPrefab);
            }
        }

        private static void ApplyMegaStats(ItemDrop.ItemData.SharedData shared)
        {
            shared.m_food = 150f;
            shared.m_foodStamina = 150f;
            shared.m_foodEitr = 150f;
            shared.m_foodBurnTime = MegaFoodConfig.Duration.Value * 60f;
            shared.m_foodRegen = 7f;
        }

        private static void ApplyCommonItemProps(ItemDrop.ItemData.SharedData shared)
        {
            shared.m_weight = 0f;
            shared.m_maxStackSize = MegaFoodConfig.StackSize.Value;
        }

        private static void ApplyMeadStatusEffect(ItemDrop.ItemData.SharedData shared)
        {
            var originalIcon = shared.m_consumeStatusEffect?.m_icon;

            // Build a fresh SE_Stats — never clone vanilla (inherits unwanted damage mods)
            var se = ScriptableObject.CreateInstance<SE_Stats>();
            se.name = "SE_MegaMead";
            se.m_name = "$se_megamead";
            se.m_tooltip = "$se_megamead_tooltip";
            se.m_icon = originalIcon;
            se.m_ttl = MegaFoodConfig.Duration.Value * 60f;
            Object.DontDestroyOnLoad(se);

            // Fire Resistance Barley Wine (BarleyWine)
            // Frost Resistance Mead (MeadFrostResist)
            // Poison Resistance Mead (MeadPoisonResist)
            var mods = new List<HitData.DamageModPair>();
            if (MegaFoodConfig.FireResistance.Value)
                mods.Add(new HitData.DamageModPair { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.VeryResistant });
            if (MegaFoodConfig.FrostResistance.Value)
                mods.Add(new HitData.DamageModPair { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.VeryResistant });
            if (MegaFoodConfig.PoisonResistance.Value)
                mods.Add(new HitData.DamageModPair { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.VeryResistant });
            se.m_mods = mods;

            // Lingering Healing Mead (MeadHealthLingering) — +25% health regen
            if (MegaFoodConfig.LingeringHealing.Value)
                se.m_healthRegenMultiplier = 1.25f;

            // Lingering Stamina Mead (MeadStaminaLingering) — +25% stamina regen
            if (MegaFoodConfig.LingeringStamina.Value)
                se.m_staminaRegenMultiplier = 1.25f;

            // Lingering Eitr Mead (MeadEitrLingering) — +25% eitr regen
            if (MegaFoodConfig.LingeringEitr.Value)
                se.m_eitrRegenMultiplier = 1.25f;

            // Love Potion (MeadTrollPheromones) — troll pheromones
            if (MegaFoodConfig.LovePotion.Value)
            {
                se.m_pheromoneFlee = true;
                var trollPrefab = ZNetScene.instance?.GetPrefab("Troll");
                if (trollPrefab != null)
                    se.m_pheromoneTarget = trollPrefab;
            }

            // Berserkir Mead (MeadBzerker) — attack, block, dodge stamina -80%
            if (MegaFoodConfig.Berserkir.Value)
            {
                se.m_attackStaminaUseModifier = -0.8f;
                se.m_blockStaminaUseModifier = -0.8f;
                // Dodge stamina handled via Harmony patch (DodgeStaminaPatch)
            }

            // Anti-Sting Concoction (MeadBugRepellent) — prevent deathsquito attacks
            if (MegaFoodConfig.AntiSting.Value)
            {
                se.m_stealthModifier = 1f;
                se.m_noiseModifier = -1f;
            }

            // Draught of Vananidir (MeadSwimmer) — -50% swim stamina
            if (MegaFoodConfig.DraughtOfVananidir.Value)
                se.m_swimStaminaUseModifier = -0.5f;

            // Tonic of Ratatosk (MeadHasty) — +15% walk/run, +7.5% swim speed
            if (MegaFoodConfig.TonicOfRatatosk.Value)
            {
                se.m_speedModifier = 0.15f;
                se.m_swimSpeedModifier = 0.075f;
            }

            // Mead of Troll Endurance (MeadStrength) — +250 carry weight
            if (MegaFoodConfig.TrollEndurance.Value)
                se.m_addMaxCarryWeight = 250f;

            // Lightfoot Mead (MeadLightfoot) — -30% jump stamina
            // Jump height +20% handled via Harmony patch (JumpHeightPatch)
            if (MegaFoodConfig.Lightfoot.Value)
                se.m_jumpStaminaUseModifier = -0.3f;

            // Brew of Animal Whispers (MeadTamer) — x2 taming via Harmony patch

            shared.m_consumeStatusEffect = se;
        }

        private static GameObject ClonePrefab(ObjectDB objectDB, string originalName, string newName)
        {
            var original = objectDB.GetItemPrefab(originalName);
            if (original == null)
                return null;

            var clone = Object.Instantiate(original, _prefabContainer.transform);
            clone.name = newName;
            return clone;
        }

        private static void ApplyPurpleTint(GameObject prefab)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        mat.color = new Color(
                            c.r * PurpleTint.r,
                            c.g * PurpleTint.g,
                            c.b * PurpleTint.b,
                            c.a);
                    }
                }
            }
        }

        #endregion

        #region Recipes

        private static void BuildRecipes(ObjectDB objectDB)
        {
            var cauldron = FindCraftingStation(objectDB, "piece_cauldron");

            BuildRecipe(objectDB, "MegaYgg", cauldron, 5, new[]
            {
                Ingredient(objectDB, "Sap", 4),
                Ingredient(objectDB, "Barley", 3),
                Ingredient(objectDB, "RoyalJelly", 2),
                Ingredient(objectDB, "SurtlingCore", 1),
            });

            BuildRecipe(objectDB, "MegaEgg", cauldron, 1, new[]
            {
                Ingredient(objectDB, "ChickenEgg", 1),
                Ingredient(objectDB, "SurtlingCore", 1),
            });

            BuildRecipe(objectDB, "MegaJerk", cauldron, 1, new[]
            {
                Ingredient(objectDB, "RawMeat", 1),
                Ingredient(objectDB, "Honey", 1),
                Ingredient(objectDB, "SurtlingCore", 1),
            });

            // MegaMead Base recipe
            BuildRecipe(objectDB, "MegaMeadBase", cauldron, 1, new[]
            {
                Ingredient(objectDB, "Honey", 10),
                Ingredient(objectDB, "Sap", 5),
                Ingredient(objectDB, "MushroomJotunPuffs", 2),
                Ingredient(objectDB, "MushroomMagecap", 5),
                Ingredient(objectDB, "SurtlingCore", 1),
            });
        }

        private static CraftingStation FindCraftingStation(ObjectDB objectDB, string prefabName)
        {
            foreach (var recipe in objectDB.m_recipes)
            {
                if (recipe?.m_craftingStation != null &&
                    recipe.m_craftingStation.gameObject.name == prefabName)
                    return recipe.m_craftingStation;
            }
            return null;
        }

        private static void BuildRecipe(ObjectDB objectDB, string itemName,
            CraftingStation station, int stationLevel, Piece.Requirement[] requirements)
        {
            var itemPrefab = objectDB.GetItemPrefab(itemName);
            if (itemPrefab == null)
                return;

            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.name = $"Recipe_{itemName}";
            recipe.m_item = itemPrefab.GetComponent<ItemDrop>();
            recipe.m_amount = 1;
            recipe.m_enabled = true;
            recipe.m_craftingStation = station;
            recipe.m_minStationLevel = stationLevel;
            recipe.m_resources = requirements;

            CachedRecipes.Add(recipe);
        }

        private static Piece.Requirement Ingredient(ObjectDB objectDB, string prefabName, int amount)
        {
            var prefab = objectDB.GetItemPrefab(prefabName);
            if (prefab == null)
                return new Piece.Requirement();

            return new Piece.Requirement
            {
                m_resItem = prefab.GetComponent<ItemDrop>(),
                m_amount = amount,
                m_amountPerLevel = 0,
                m_recover = true,
            };
        }

        #endregion
    }
}
