# Lifeblood Engine

**Lifeblood Engine** is a full-featured 3D PvP shooter engine in the style of Quake World, featuring:
- 🎮 Native 3D engine built on OpenGL
- 🏃 Quake-style physics (bunny hopping, strafe jumping)
- 🌐 High-quality networking code for multiplayer
- 🗺️ 3D maps and models support

## Quick Start

### Requirements
- Windows 10/11
- .NET Framework v4.0.30319
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

> **The Ultimate Half-Life / Quake / CS Hybrid Engine in Pure C#**
Lifeblood is a high-performance, raw FPS engine built from scratch using **native OpenGL 1.1** (via `opengl32.dll` P/Invoke) and **Windows Forms**. It features Quake-style movement, Counter-Strike gunplay, and a custom 3D engine without any external frameworks (No Unity, No Unreal).

## 🚀 Key Features

### 1. Custom 3D Engine & Graphics
-   **Native Rendering**: Pure C# OpenGL 1.1 pipeline.
-   **Assets**: Loads `.obj` 3D Models and `.png` Textures.
-   **Procedural Arena**: Generates a 400x400 map with walls, pillars, and cover.
-   **Settings**: Change **Resolution**, **FOV**, and Toggle Fullscreen.

### 2. "Ideal" Movement (ProMode Physics)
-   **Air Control**: Full CPMA-style air turning (turn in air without losing speed).
-   **Bunny Hopping**: Uncapped speed limits with proper Strafe Jumping.
-   **Rocket Jump**: Physics-based explosive impulse support (`ApplyImpulse`).
-   **Wall Run**: Vector-based detection (`Dot Product`).

### 3. Lethal Gunplay & Arsenal
-   **Weapons**: Knife, Pistol, Deagle, Shotgun, AK-47, Scout, Rocket Launcher.
-   **Stats System**: Tracks **Kills**, **Deaths**, **Damage**, **Headshot %**, **Accuracy**.
-   **Scoreboard**: Hold `TAB` to see stats.
-   **Crosshair**: Fully customizable CS-style crosshair (Size, Gap, Dot, Color, Alpha).

### 4. Game Modes
-   **Survival**: Infinite waves of enemies. Death resets the run (Roguelike).
-   **Builder Mode (F1)**: Place/Remove blocks (Voxels) and save maps to `custom_map.txt`.
-   **Multiplayer**: Client-Server architecture (UDP) with simulated Download Manager.

### 5. Modding & Networking
-   **Mod Loader**: Scans `bin/mods/` for content.
-   **Download Manager**: Simulates downloading missing assets (maps/models) from server.
-   **Netcode**: Input Prediction architecture ready.

## 🎮 Controls
| Key | Action |
| :--- | :--- |
| **WASD** | Movement (Strafe enabled) |
| **Space** | Jump / Auto-Bhop |
| **Mouse** | Aim & Shoot |
| **TAB** | Scoreboard |
| **1-7** | Select Weapon |
| **F1** | Builder Mode |
| **P / L** | Save / Load Map |

## 🛠️ Build & Installation

### ⚠️ Important Note on `bin` Folder
**Q: If I delete the `bin` folder, will the script work?**
A: **Yes and No.**
-   **Yes**, the script `build.bat` will automatically **recreate** the `bin` folder and recompile the `Lifeblood.exe` game executable from the source code. The game logic will work perfectly.
-   **No**, if you had `bin/content` (Models/Textures) or `bin/mods`, they **will be lost** and `build.bat` does significant restore them (unless you have a separate backup or asset generation script).
-   **Recommendation**: Do not delete `bin` unless you are sure you have backups of your assets (`bin/content`).

## ⚙️ Configuration
You can edit settings via the in-game **Settings Menu** or manually in `Game/Settings.cs`.
-   **Nickname**: Set your player name.
-   **Video**: Resolution, FOV.
-   **Crosshair**: Style, Size, Color.
-   **Input**: Sensitivity.

## 🔗 Connect with me
[![YouTube](https://img.shields.io/badge/YouTube-@adiruaim-FF0000?style=for-the-badge&logo=youtube)](https://www.youtube.com/@adiruaim)
[![TikTok](https://img.shields.io/badge/TikTok-@adiruhs-000000?style=for-the-badge&logo=tiktok)](https://www.tiktok.com/@adiruhs)

### 💰 Legacy Crypto
* **BTC:** `bc1qflvetccw7vu59mq074hnvf03j02sjjf9t5dphl`
* **ETH:** `0xf35Afdf42C8bf1C3bF08862f573c2358461e697f`
* **Solana:** `5r2H3R2wXmA1JimpCypmoWLh8eGmdZA6VWjuit3AuBkq`
* **USDT (TRC20):** `TNgFjGzbGxztHDcSHx9DEPmQLxj2dWzozC`
* **USDT (TON):** `UQC5fsX4zON_FgW4I6iVrxVDtsVwrcOmqbjsYA4TrQh3aOvj`

### 🌍 Support Links
[![Donatello](https://img.shields.io/badge/Support-Donatello-orange?style=for-the-badge)](https://donatello.to/Adiru3)
[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support-blue?style=for-the-badge&logo=kofi)](https://ko-fi.com/adiru)

[![Steam](https://img.shields.io/badge/Steam-Trade-blue?style=for-the-badge&logo=steam)](https://steamcommunity.com/tradeoffer/new/?partner=1124211419&token=2utLCl48)
