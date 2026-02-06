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
            {
                Plugin.Log.LogWarning("RegisterAll skipped — ObjectDB not ready yet " +
                    $"(items={objectDB.m_items.Count})");
                return;
            }

            if (!_prefabsCreated)
            {
                InitContainer();

                Plugin.Log.LogInfo("Creating MegaFood prefabs...");
                CreateFood(objectDB, "YggdrasilPorridge", "MegaYgg",
                    "$item_megaygg", "$item_megaygg_desc", MegaFoodConfig.MegaYgg);
                CreateFood(objectDB, "CookedEgg", "MegaEgg",
                    "$item_megaegg", "$item_megaegg_desc", MegaFoodConfig.MegaEgg);
                CreateFood(objectDB, "BoarJerky", "MegaJerk",
                    "$item_megajerk", "$item_megajerk_desc", MegaFoodConfig.MegaJerk);

                CreateMead(objectDB);
                _prefabsCreated = true;
            }

            // (Re-)inject into ObjectDB — CopyOtherDB wipes the lists each time
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
            {
                objectDB.UpdateRegisters();
                Plugin.Log.LogInfo($"Injected {added} items into ObjectDB");
            }

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
                    Plugin.Log.LogDebug($"ZNetScene registered: {prefab.name}");
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
            Plugin.Log.LogDebug("Fermenter conversion added: MegaMeadBase ? MegaMead");
        }

        #region Prefab creation

        private static void InitContainer()
        {
            _prefabContainer = new GameObject("MegaFood_Prefabs");
            _prefabContainer.SetActive(false);
            Object.DontDestroyOnLoad(_prefabContainer);
        }

        private static void CreateFood(ObjectDB objectDB, string basePrefab,
            string newName, string nameToken, string descToken, FoodStats stats)
        {
            var clone = ClonePrefab(objectDB, basePrefab, newName);
            if (clone == null) return;

            var shared = clone.GetComponent<ItemDrop>().m_itemData.m_shared;
            shared.m_name = nameToken;
            shared.m_description = descToken;
            ApplyMegaStats(shared, stats);
            ApplyCommonItemProps(shared);
            ApplyPurpleTint(clone);

            CachedPrefabs.Add(clone);
            Plugin.Log.LogInfo($"Created {newName} (base={basePrefab}) — " +
                $"HP={stats.Health.Value} Stam={stats.Stamina.Value} Eitr={stats.Eitr.Value}");
        }

        private static void CreateMead(ObjectDB objectDB)
        {
            // MegaMead Base — craftable at cauldron, goes into fermenter
            MegaMeadBasePrefab = ClonePrefab(objectDB, "MeadBaseEitrMinor", "MegaMeadBase");
            if (MegaMeadBasePrefab != null)
            {
                var baseShared = MegaMeadBasePrefab.GetComponent<ItemDrop>().m_itemData.m_shared;
                baseShared.m_name = "$item_megameadbase";
                baseShared.m_description = "$item_megameadbase_desc";
                ApplyCommonItemProps(baseShared);
                ApplyPurpleTint(MegaMeadBasePrefab);
                CachedPrefabs.Add(MegaMeadBasePrefab);
                Plugin.Log.LogInfo("Created MegaMeadBase (base=MeadBaseEitrMinor)");
            }

            // MegaMead — produced by fermenter, drinkable
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
                Plugin.Log.LogInfo("Created MegaMead (base=MeadEitrMinor)");
            }
        }

        private static void ApplyMegaStats(ItemDrop.ItemData.SharedData shared, FoodStats stats)
        {
            shared.m_food = stats.Health.Value;
            shared.m_foodStamina = stats.Stamina.Value;
            shared.m_foodEitr = stats.Eitr.Value;
            shared.m_foodBurnTime = 3600f;  // 1 hour
            shared.m_foodRegen = 7f;
        }

        private static void ApplyCommonItemProps(ItemDrop.ItemData.SharedData shared)
        {
            shared.m_weight = 0f;
            shared.m_maxStackSize = MegaFoodConfig.StackSize.Value;
        }

        private static void ApplyMeadStatusEffect(ItemDrop.ItemData.SharedData shared)
        {
            var origSE = shared.m_consumeStatusEffect;
            if (origSE == null)
            {
                Plugin.Log.LogWarning("MegaMead: original StatusEffect is null — creating new SE_Stats");
                origSE = ScriptableObject.CreateInstance<SE_Stats>();
            }

            var se = Object.Instantiate(origSE);
            se.name = "SE_MegaMead";
            se.m_name = "$se_megamead";
            se.m_tooltip = "$se_megamead_tooltip";
            Object.DontDestroyOnLoad(se);

            if (se is SE_Stats seStats)
            {
                var cfg = MegaFoodConfig.MegaMead;
                seStats.m_staminaRegenMultiplier = 1f + cfg.StaminaRegen.Value / 100f;
                seStats.m_eitrRegenMultiplier    = 1f + cfg.EitrRegen.Value / 100f;
                seStats.m_healthRegenMultiplier  = 1f + cfg.HealthRegen.Value / 100f;
                seStats.m_runStaminaDrainModifier    = -(cfg.StaminaReduction.Value / 100f);
                seStats.m_attackStaminaUseModifier   = -(cfg.StaminaReduction.Value / 100f);
                seStats.m_blockStaminaUseModifier    = -(cfg.StaminaReduction.Value / 100f);

                Plugin.Log.LogInfo($"MegaMead SE: staminaRegen={seStats.m_staminaRegenMultiplier}, " +
                    $"eitrRegen={seStats.m_eitrRegenMultiplier}, healthRegen={seStats.m_healthRegenMultiplier}");
            }

            shared.m_consumeStatusEffect = se;
        }

        private static GameObject ClonePrefab(ObjectDB objectDB, string originalName, string newName)
        {
            var original = objectDB.GetItemPrefab(originalName);
            if (original == null)
            {
                Plugin.Log.LogError($"Base prefab NOT FOUND: {originalName}");
                return null;
            }

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
            if (cauldron == null)
                Plugin.Log.LogError("Cauldron crafting station NOT FOUND");
            else
                Plugin.Log.LogDebug($"Found cauldron: {cauldron.gameObject.name}");

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
            {
                Plugin.Log.LogError($"Recipe target prefab NOT FOUND: {itemName}");
                return;
            }

            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.name = $"Recipe_{itemName}";
            recipe.m_item = itemPrefab.GetComponent<ItemDrop>();
            recipe.m_amount = 1;
            recipe.m_enabled = true;
            recipe.m_craftingStation = station;
            recipe.m_minStationLevel = stationLevel;
            recipe.m_resources = requirements;

            CachedRecipes.Add(recipe);
            Plugin.Log.LogInfo($"Built recipe: {recipe.name} (station={station?.gameObject.name}, level={stationLevel})");
        }

        private static Piece.Requirement Ingredient(ObjectDB objectDB, string prefabName, int amount)
        {
            var prefab = objectDB.GetItemPrefab(prefabName);
            if (prefab == null)
            {
                Plugin.Log.LogError($"Ingredient prefab NOT FOUND: {prefabName}");
                return new Piece.Requirement();
            }

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
