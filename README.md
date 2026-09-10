# GrimmchildCoopMod

**Version 1.1.0**

Cooperative Mod to turn the original Grimmchild into a fully playable companion controlled by a second player. Designed for local cooperative play while preserving the original feel of Hollow Knight.

<img width="1920" height="1080" alt="Sin título" src="https://github.com/user-attachments/assets/64b13995-ae64-48e0-83cd-30a9c2084189" />

---

## Features

- The Knight is controlled by Player 1.
- Grimmchild is controlled by Player 2.
- Manual movement.
- Manual attack.
- Automatic teleport when too far from the Knight.
- Original bench behavior is preserved.
- Grimmchild remains permanently at Level 4.
- The Grimmchild charm does not consume a notch.
- Configurable damage:
  - Original Level 4 Grimmchild damage.
  - Damage scales with the Knight's current Nail level.

---

## In-Game Options

<img width="808" height="251" alt="Captura de pantalla 2026-09-10 111906" src="https://github.com/user-attachments/assets/5908cde3-c7f5-42c8-8348-741b1f30f2d0" />


The mod includes an in-game settings menu where you can customize Grimmchild's controller, damage, and vulnerability.

### Controller

Choose which connected controller is used to control Grimmchild.

- **Controller 1** — Grimmchild is controlled by the first controller.
- **Controller 2** — Grimmchild is controlled by the second controller.

The other controller will automatically be assigned to the Knight.

### Grimmchild Damage

Choose how much damage Grimmchild deals.

- **Original** — Uses the default Level 4 Grimmchild damage.
- **Scale with Nail** — Grimmchild deals the same damage as the Knight's current Nail.

### Grimmchild Vulnerability

Choose whether Grimmchild can take damage from enemies, projectiles, and hazards.

- **Off** — Grimmchild cannot take damage.
- **On** — Grimmchild can take damage. When defeated, Grimmchild will disappear and revive when resting at a bench. If the Knight dies while Grimmchild is defeated, Grimmchild will respawn with the Knight.

<img width="943" height="449" alt="Captura de pantalla 2026-09-10 111936" src="https://github.com/user-attachments/assets/c6583d26-e395-44f4-b4a0-91d7f4574709" />



---

## Requirements

- Hollow Knight **1.5.78.11833**
- Hollow Knight Modding API ([Lumafly](https://themulhima.github.io/Lumafly/) recommended)
- Two XInput-compatible controllers

---

## Manual Installation

1. Install the Hollow Knight Modding API.
2. Create a folder named `GrimmchildCoopMod` inside the `Mods` folder.
3. Copy `GrimmchildCoopMod.dll` into that folder.
4. Launch Hollow Knight with both controllers connected.

---

## Controls

### Player 1

Controls the Knight normally.

### Player 2

### Move

Left Stick

<img width="384" height="288" alt="Movement" src="https://github.com/user-attachments/assets/fdc01ce6-9f93-4e27-9952-b96d1da3cb98" />

### Attack

Xbox **X** Button

<img width="384" height="288" alt="Attack" src="https://github.com/user-attachments/assets/3a21adbe-aeed-44af-ab1d-24b4c574e18e" />

### Teleport

Automatic when Grimmchild gets too far from the Knight.

<img width="384" height="288" alt="Teleport" src="https://github.com/user-attachments/assets/7fdfc9b0-102b-40db-a413-beaa9a767620" />

---

## Known Limitations

- Grimmchild is permanently forced to Level 4, so parts of the Grimm Troupe progression may not behave as intended.
- Two XInput-compatible controllers are required.
- Grimmchild currently uses the original sprites.
- On a brand-new save file, you must save, quit, and reload once for Grimmchild to appear.

---

## Planned Features

- Custom Grimmchild sprites.
- Additional gameplay options.
- More configurable settings.
- General improvements and polish.

---

## License

This project is licensed under the MIT License.

---

## Credits

- Team Cherry for Hollow Knight.
- The Hollow Knight Modding community.
- Lumafly.
