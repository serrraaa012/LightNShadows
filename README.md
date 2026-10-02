# Light(N)Shadows 🌗

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-brightgreen.svg?logo=windows)](https://github.com/serrraaa012/LightNShadows)
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](LICENSE)

> A high-speed 2D dual-dimension endless runner and precision platformer built with **Unity 6**. Master instantaneous realm shifting, dodge lethal physical hazards, and ghost through matching polarity energy fields.

---

## 🎮 Gameplay Overview

In **Light(N)Shadows**, reality is split into two coexisting states:
- ☀️ **The Light Realm:** A radiant morning environment imbued with warm solar gold energy.
- 🌑 **The Shadow Realm:** A deep midnight metropolis bathed in cool cyan and obsidian moonlight.

As your runner accelerates along the track, you must perform split-second actions: **jump** over neutral physical spikes, and **phase-shift** between Light and Shadow dimensions to ghost harmlessly through matching laser gates.

---

## 🕹️ Controls

| Action | Primary Key | Secondary / Alternative | Description |
| :--- | :--- | :--- | :--- |
| **JUMP** | `W` | `Up Arrow` (`↑`) | Springs the ball into the air to clear ground spikes. |
| **PHASE SHIFT** | `Spacebar` | `Left Click` / Screen Tap | Instantly toggles between the **Light Realm** and **Shadow Realm**. |
| **PAUSE / RESUME** | `Escape` (`Esc`) | `P` / Top-Right Button (`⏸`) | Freezes gameplay and opens the pause menu. |
| **UI NAVIGATION** | Mouse Click | Enter / Click | Navigate menus, enter custom runner names, restart runs. |

---

## ⚡ Dual-Realm Polarity Mechanics
       ┌────────────────────────────────────────────────────────┐
         │                  LIGHT(N)SHADOWS                      │
         │              DUAL-DIMENSION POLARITY                   │
         └────────────────────────────────────────────────────────┘

[ LIGHT REALM ACTIVE ]                          [ SHADOW REALM ACTIVE ]
  Golden Amber Aura                               Cyan Void Aura
        │                                               │
        ├──► Light Laser Gate:   PHASE (SAFE)           ├──► Light Laser Gate:   LETHAL (CRASH)
        ├──► Shadow Laser Gate:  LETHAL (CRASH)         ├──► Shadow Laser Gate:  PHASE (SAFE)
        └──► Neutral Red Spikes: LETHAL (MUST JUMP)     └──► Neutral Red Spikes: LETHAL (MUST JUMP)
        
- **Matching Realm (Phase):** Passing through an energy barrier of the same polarity allows you to ghost through unharmed while earning passage points.
- **Opposing Realm (Crash):** Colliding with an unmatched energy gate immediately shatters the energy core and ends the run.
- **Neutral Hazards (Red Spikes):** Physical matter that exists across both dimensions. **Cannot be phased**; must be leaped over using Jump (`W` / `↑`).

---

## 🎯 Hazards & Collectibles

| Hazard / Item | Realm Type | Action Required |
| :--- | :--- | :--- |
| **Crystal Spike** | Neutral (Crimson Red) | **JUMP** (`W` or `↑`). Solid in all realms. |
| **Laser Gate** | Light (Gold) / Shadow (Cyan) | **PHASE SHIFT** (`Space` / `Click`) to match its color. |
| **Ceiling Spire** | Neutral (Hanging Stalactite) | **RUN UNDER**. Safe on ground; avoid jumping into it. |
| **Floating Diamond** | Light / Shadow (Mid-Air) | Match realm to phase through if jumping near it. |
| **Prism Orb** | Collectible (Golden/Cyan) | Touch to collect for bonus points. |

---

## 🌟 Key Features

- **Dynamic Crossfading Environments:** Real-time seamless visual blending between daytime and night backgrounds upon dimension shifting.
- **Custom Soundtrack & Audio:** Custom high-energy streaming background music and tactile button click sound effects.
- **Kinetic Elastic Ball Physics:** Squash-and-stretch animations on jumping/landing, forward rolling rotation, orbiting halo, and chromatic shockwaves.
- **Personalized High Scores:** Multiple runner profiles supported with per-name local persistent high-score tracking.
- **Modern Cyber-Glass HUD:** Responsive interface displaying Current Score, Personal Best, and runner badges.

---

🛠️ Tech Stack
Engine: Unity 6 (Universal 2D Pipeline)
Language: C# (.NET Standard 2.1)
Physics: Unity 2D Physics & Custom Kinematic Jump Curves
Audio: Custom Multi-tier Sound Manager (Inspector + Streaming WebRequest Fallback)
Persistence: Unity PlayerPrefs with Name-keyed Records

---

## 🚀 Getting Started

### Prerequisites
- [Unity 6](https://unity.com/releases/editor) (Version `6000.0.x` or compatible)
- Windows 10/11 (64-bit)

### Installation & Running in Editor
1. Clone the repository:
   ```bash
   git clone https://github.com/serrraaa012/LightNShadows.git
