using System.Collections.Generic;
using UnityEngine;

namespace MegaFood
{
    public static class MegaFoodItems
    {
        private static readonly Color PurpleTint = new Color(0.75f, 0.5f, 1.0f);

        private static readonly List<GameObject> RegisteredPrefabs = new List<GameObject>();

        public static void RegisterAll(ObjectDB objectDB)
        {
            if (objectDB.m_items.Count == 0 || objectDB.GetItemPrefab("Wood") == null)
                return;

            if (objectDB.GetItemPrefab("MegaYgg") != null)
                return;

            var megaYgg = CreateFood(objectDB, "YggdrasilPorridge", "MegaYgg",
                "$item_megaygg", "$item_megaygg_desc", MegaFoodConfig.MegaYgg);
            var megaEgg = CreateFood(objectDB, "CookedEgg", "MegaEgg",
                "$item_megaegg", "$item_megaegg_desc", MegaFoodConfig.MegaEgg);
            var megaJerk = CreateFood(objectDB, "BoarJerky", "MegaJerk",
                "$item_megajerk", "$item_megajerk_desc", MegaFoodConfig.MegaJerk);

            if (megaYgg != null) RegisterItem(objectDB, megaYgg);
            if (megaEgg != null) RegisterItem(objectDB, megaEgg);
            if (megaJerk != null) RegisterItem(objectDB, megaJerk);

            objectDB.UpdateRegisters();

            AddRecipes(objectDB);
        }

        public static void RegisterPrefabs(ZNetScene zNetScene)
        {
            foreach (var prefab in RegisteredPrefabs)
            {
                if (prefab != null && !zNetScene.m_prefabs.Contains(prefab))
                {
                    zNetScene.m_prefabs.Add(prefab);
                    zNetScene.m_namedPrefabs[prefab.name.GetStableHashCode()] = prefab;
                }
            }
        }

        private static GameObject CreateFood(ObjectDB objectDB, string basePrefab,
            string newName, string nameToken, string descToken, FoodStats stats)
        {
            var clone = ClonePrefab(objectDB, basePrefab, newName);
            if (clone == null) return null;

            var shared = clone.GetComponent<ItemDrop>().m_itemData.m_shared;
            shared.m_name = nameToken;
            shared.m_description = descToken;
            ApplyMegaStats(shared, stats);
            ApplyPurpleTint(clone);
            TintIcons(shared);

            return clone;
        }

        private static void ApplyMegaStats(ItemDrop.ItemData.SharedData shared, FoodStats stats)
        {
            shared.m_food = stats.Health.Value;
            shared.m_foodStamina = stats.Stamina.Value;
            shared.m_foodEitr = stats.Eitr.Value;
            shared.m_foodBurnTime = 2400f;  // 40 minutes
            shared.m_foodRegen = 7f;
        }

        private static GameObject ClonePrefab(ObjectDB objectDB, string originalName, string newName)
        {
            var original = objectDB.GetItemPrefab(originalName);
            if (original == null)
            {
                Debug.LogWarning($"[MegaFood] Could not find base prefab: {originalName}");
                return null;
            }

            var clone = Object.Instantiate(original);
            clone.name = newName;
            Object.DontDestroyOnLoad(clone);
            clone.SetActive(true);

            return clone;
        }

        private static void RegisterItem(ObjectDB objectDB, GameObject prefab)
        {
            objectDB.m_items.Add(prefab);
            RegisteredPrefabs.Add(prefab);

            var zNetScene = ZNetScene.instance;
            if (zNetScene != null && !zNetScene.m_prefabs.Contains(prefab))
            {
                zNetScene.m_prefabs.Add(prefab);
                zNetScene.m_namedPrefabs[prefab.name.GetStableHashCode()] = prefab;
            }
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

        private static void TintIcons(ItemDrop.ItemData.SharedData shared)
        {
            if (shared.m_icons == null || shared.m_icons.Length == 0)
                return;

            for (int i = 0; i < shared.m_icons.Length; i++)
            {
                var original = shared.m_icons[i];
                if (original == null || original.texture == null) continue;

                var tinted = CreateTintedSprite(original, PurpleTint);
                if (tinted != null)
                    shared.m_icons[i] = tinted;
            }
        }

        private static Sprite CreateTintedSprite(Sprite original, Color tint)
        {
            var rt = RenderTexture.GetTemporary(
                original.texture.width, original.texture.height, 0);
            Graphics.Blit(original.texture, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var tex = new Texture2D(
                original.texture.width, original.texture.height,
                TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

            var pixels = tex.GetPixels();
            for (int j = 0; j < pixels.Length; j++)
            {
                pixels[j] = new Color(
                    pixels[j].r * tint.r,
                    pixels[j].g * tint.g,
                    pixels[j].b * tint.b,
                    pixels[j].a);
            }
            tex.SetPixels(pixels);
            tex.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
        }

        #region Recipes

        private static void AddRecipes(ObjectDB objectDB)
        {
            var cauldron = FindCraftingStation(objectDB, "piece_cauldron");

            // MegaYgg - Cauldron level 5 (same as Yggdrasil Porridge)
            AddRecipe(objectDB, "MegaYgg", cauldron, 5, new[]
            {
                Ingredient(objectDB, "Sap", 4),
                Ingredient(objectDB, "Barley", 3),
                Ingredient(objectDB, "RoyalJelly", 2),
                Ingredient(objectDB, "SurtlingCore", 1),
            });

            // MegaEgg - Cauldron level 1 (CookedEgg normally uses a cooking station,
            // but multi-ingredient recipes require a cauldron)
            AddRecipe(objectDB, "MegaEgg", cauldron, 1, new[]
            {
                Ingredient(objectDB, "ChickenEgg", 1),
                Ingredient(objectDB, "SurtlingCore", 1),
            });

            // MegaJerk - Cauldron level 1 (same as Boar Jerky)
            AddRecipe(objectDB, "MegaJerk", cauldron, 1, new[]
            {
                Ingredient(objectDB, "RawMeat", 1),
                Ingredient(objectDB, "Honey", 1),
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

        private static void AddRecipe(ObjectDB objectDB, string itemName,
            CraftingStation station, int stationLevel, Piece.Requirement[] requirements)
        {
            var itemPrefab = objectDB.GetItemPrefab(itemName);
            if (itemPrefab == null) return;

            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.name = $"Recipe_{itemName}";
            recipe.m_item = itemPrefab.GetComponent<ItemDrop>();
            recipe.m_amount = 1;
            recipe.m_enabled = true;
            recipe.m_craftingStation = station;
            recipe.m_minStationLevel = stationLevel;
            recipe.m_resources = requirements;

            objectDB.m_recipes.Add(recipe);
        }

        private static Piece.Requirement Ingredient(ObjectDB objectDB, string prefabName, int amount)
        {
            var prefab = objectDB.GetItemPrefab(prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"[MegaFood] Could not find ingredient prefab: {prefabName}");
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
