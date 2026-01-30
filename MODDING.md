# Lifeblood Modding Guide

Welcome to the Lifeblood Creation Kit. You can add your own 3D weapon models and textures easily.

## Structure
To create a mod, create a folder inside the `bin/mods/` directory (e.g., `bin/mods/MyCoolMod/`).

Inside your mod folder, you can have:
- `models/` - For .obj 3D models.
- `textures/` - For .png textures.

## Adding Custom Weapons
The game automatically looks for models that match weapon names. 
Currently supported identifiers:
- `knife`
- `pistol`
- `shotgun`
- `machinegun` (AK/SMG)
- `railgun`
- `rocketlauncher`
- `shaft` (Lightning Gun)

 **How to override:**
1. Place your `.obj` file in `bin/mods/MyMod/models/`.
2. Name it `railgun.obj` (to replace the Railgun model).
3. The game will automatically load it on startup and use it for the viewmodel.

**Note:** Models should be centered at (0,0,0) and facing -Z usually. You may need to adjust scale/rotation in your 3D software (Blender).

## Texture Overrides
You can override internal textures by placing `.png` files in `bin/mods/MyMod/textures/`.
- `default.png` - Overrides the fallback checkerboard.
- Texture names must match what the game expects (currently mostly procedural/internal, more support coming).
