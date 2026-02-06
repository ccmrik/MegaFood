# MegaFood – Valheim Mod

## Project Overview

This is a **Valheim mod** built with **BepInEx 5.x** and **Harmony**.
It adds three custom, high-tier food items and a mead to the game, each with a distinctive purple hue.

## Architecture

| File | Purpose |
|---|---|
| `Plugin.cs` | BepInEx plugin entry point — initialises config and Harmony patches |
| `MegaFoodConfig.cs` | BepInEx `ConfigEntry` bindings for per-item Health / Stamina / Eitr |
| `MegaFoodItems.cs` | Clones existing prefabs, applies stats/tint, creates recipes |
| `GamePatches.cs` | Harmony postfix patches for ObjectDB, ZNetScene, Localization, Fermenter |

## Coding Conventions

- **Target framework:** `netstandard2.1` (required by the Unity / Mono runtime Valheim uses).
- **Harmony patches** use `[HarmonyPatch]` attribute style — one class per patch target.
- Localisation keys are prefixed `item_mega` (e.g. `$item_megaygg`).
- All MegaFood items read Health / Stamina / Eitr from the BepInEx config (`MegaFoodConfig.cs`). Duration and regen are still in `ApplyMegaStats()`.
- Cloned prefabs are marked `DontDestroyOnLoad` and guarded against double-registration.
- Game assemblies are referenced from the local Valheim install via `Environment.props` (git-ignored).
- `Private="false"` on all game/engine references — they must NOT be copied to output.

## Internal Prefab Names (Valheim)

| Display Name | Prefab Name |
|---|---|
| Yggdrasil Porridge | `YggdrasilPorridge` |
| Cooked Egg | `CookedEgg` |
| Boar Jerky | `BoarJerky` |
| Cauldron | `piece_cauldron` |
| Minor Eitr Mead | `MeadEitrMinor` |
| Minor Eitr Mead Base | `MeadBaseEitrMinor` |

## Build & Deploy

- **Post-build** automatically copies `MegaFood.dll` to the configured r2modman plugin folder.
- **Version** is maintained in a single place: `<Version>` in `MegaFood.csproj`.
  When bumping, also update `PluginVersion` in `Plugin.cs`.
- **Environment.props** holds machine-specific paths and is git-ignored.
  Copy `Environment.props.template` for a fresh setup.
