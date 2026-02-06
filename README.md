# MegaFood – Valheim Mod

Adds three powerful food items to Valheim, each with a distinctive purple hue.

## Items

| Item | Based On | Ingredients | Stats |
|---|---|---|---|
| **MegaYgg** | Yggdrasil Porridge | 4 Sap · 3 Barley · 2 Royal Jelly · 1 Surtling Core | 150 HP / 150 Stamina / 150 Eitr |
| **MegaEgg** | Cooked Egg | 1 Egg · 1 Surtling Core | 150 HP / 150 Stamina / 150 Eitr |
| **MegaJerk** | Boar Jerky | 1 Boar Meat · 1 Honey · 1 Surtling Core | 150 HP / 150 Stamina / 150 Eitr |

All items last **40 minutes** (2 400 s) and have **7 HP/tick** regen.

## Configuration

After the first run, BepInEx generates:

```
BepInEx/config/com.rikal.megafood.cfg
```

Open it in a text editor **or** use the R2ModMan config editor. Each food has its own section:

```ini
[MegaYgg]
Health = 150
Stamina = 150
Eitr = 150

[MegaEgg]
Health = 150
Stamina = 150
Eitr = 150

[MegaJerk]
Health = 150
Stamina = 150
Eitr = 150
```

Changes take effect on the next game launch.

## Requirements

- Valheim (Steam)
- BepInEx 5.4.x (via r2modman or manual install)

## Building

1. Copy `Environment.props.template` ? `Environment.props`
2. Update the paths in `Environment.props` to match your system
3. Build the solution — the DLL is **auto-published** to your plugin folder

## Installation (manual)

Copy `MegaFood.dll` into:

```
<BepInEx>/plugins/MegaFood/MegaFood.dll
```

## License

MIT
