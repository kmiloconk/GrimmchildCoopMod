# GrimmchildCoopMod

**Version 1.4.2**

GrimmchildCoopMod turns the original Grimmchild into a fully playable companion controlled by a second player.

Designed for local cooperative play while preserving the original look, animations, sounds, and behavior of Grimmchild as much as possible.

<img width="1919" height="1079" alt="Grimmchild Co-op HUD" src="https://github.com/user-attachments/assets/c430024f-c5d0-4397-b94d-0fb1cff5ad31" />

---

## Features

- Local co-op support for two controllers.
- The Knight and Grimmchild can be assigned independently to either controller.
- Full manual control of Grimmchild's movement and attacks.
- Automatic teleport when Grimmchild gets too far from the Knight.
- Grimmchild sleeps alongside the Knight when resting at a bench.
- Optional vulnerability to enemies, projectiles, and hazards.
- Complete health, death, and revival system for Grimmchild.
- Custom Grimmchild health HUD based on the original Grimm Troupe visuals.
- Grimmchild's maximum health scales with the Knight's maximum health.
- Grimmchild loses one flame from the HUD whenever damage is taken.
- Grimmchild's health and HUD persist between rooms.
- Health is fully restored when resting at a bench.
- If Grimmchild is defeated, they remain dead between rooms until revived.
- Grimmchild revives when the Knight rests at a bench.
- If the Knight dies while Grimmchild is defeated, both respawn together.
- Grimmchild uses the original disappearance animation, effects, and sounds when defeated.
- Configurable damage:
  - Original Level 4 Grimmchild damage.
  - Damage scales with the Knight's current Nail.
- Grimmchild remains permanently at Level 4.
- The Grimmchild charm is always equipped and does not consume any notches.

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

## Gameplay Mechanics

### Knight

The Knight is controlled normally using the controller assigned to them.

### Grimmchild

The second player takes direct control of Grimmchild.

### Movement

Use the **Left Stick** to freely move Grimmchild.

<img width="384" height="288" alt="Movement" src="https://github.com/user-attachments/assets/fdc01ce6-9f93-4e27-9952-b96d1da3cb98" />

### Attack

Press the Xbox **X** button to fire Grimmchild's projectile attack.

<img width="384" height="288" alt="Attack" src="https://github.com/user-attachments/assets/3a21adbe-aeed-44af-ab1d-24b4c574e18e" />

### Teleport

If Grimmchild gets too far away from the Knight, the original Grimmchild teleport sequence is triggered automatically to bring them back.

<img width="384" height="288" alt="tpnew" src="https://github.com/user-attachments/assets/0a9e9c2f-ac4d-4298-ac27-9918a34b727e" />


### Resting

When the Knight rests at a bench, Grimmchild lands and sleeps beside them.

Grimmchild's health is fully restored while resting.

<img width="384" height="288" alt="Grimmchild sleeping" src="https://github.com/user-attachments/assets/f2e71050-54a7-47db-bc38-72407ab6803b" />

### Health

When **Grimmchild Vulnerability** is enabled, Grimmchild has their own health system.

Grimmchild's maximum health is based on the Knight's current maximum health. Taking damage removes one health point and one flame from the Grimmchild HUD.

Lost flames are restored when Grimmchild recovers health at a bench.

Grimmchild's remaining health persists when moving between rooms.

<img width="306" height="116" alt="recarganew1" src="https://github.com/user-attachments/assets/9fd33d52-03f4-427f-adfd-3a4e04eeb30e" />.


<img width="384" height="288" alt="hit" src="https://github.com/user-attachments/assets/d064a7a3-31f2-4c16-8f1d-2f2e581bc38e" />


### Death

When Grimmchild reaches zero health, they are defeated and disappear using their original Grimmchild despawn animation, sound, and visual effect.

Grimmchild remains defeated when moving between rooms and cannot be controlled until revived.

Resting at a bench revives Grimmchild with full health.

If the Knight dies while Grimmchild is already defeated, Grimmchild will respawn alongside the Knight. Grimmchild initially appears sleeping beside the Knight and wakes up when the Knight leaves the bench.

<img width="384" height="288" alt="deadnew" src="https://github.com/user-attachments/assets/ce50436f-b02b-4c15-922e-54db52f3a781" />


---

## Grimmchild HUD

Grimmchild has a dedicated health display inspired by the original Grimm Troupe interface.

The flames represent Grimmchild's remaining health:

- Each flame represents one HP.
- Taking damage removes one flame.
- Lost flames return when health is restored.
- The HUD updates automatically if the Knight gains additional permanent health.
- Grimmchild's current health and flames persist between rooms.
- When Grimmchild is defeated, the HUD reflects their defeated state.

<img width="1916" height="256" alt="Captura de pantalla 2026-10-05 235443" src="https://github.com/user-attachments/assets/513fdec0-60e0-4859-acf7-ee62ec786ec2" />
<img width="1919" height="254" alt="hud full" src="https://github.com/user-attachments/assets/f310123c-0602-46ce-aff4-5fc9def9df6a" />

---

## In-Game Options

<img width="877" height="354" alt="Captura de pantalla 2026-10-06 003528" src="https://github.com/user-attachments/assets/a364da41-316b-48af-aead-e98532c7dc5d" />

The mod includes an in-game settings menu where you can customize Grimmchild's controller, damage, and vulnerability.

### Controller

Choose which connected controller is used to control Grimmchild.

- **Controller 1** — Grimmchild is controlled by the first controller.
- **Controller 2** — Grimmchild is controlled by the second controller.

The other controller is assigned to the Knight.

### Grimmchild Damage

Choose how much damage Grimmchild deals.

- **Original** — Uses the original Level 4 Grimmchild damage.
- **Scale with Nail** — Grimmchild deals the same damage as the Knight's current Nail.

### Grimmchild Vulnerability

Choose whether Grimmchild can take damage.

- **Off** — Grimmchild cannot be damaged.
- **On** — Grimmchild can take damage from enemies, projectiles, and hazards and uses the health, death, and revival systems.

<img width="943" height="449" alt="Grimmchild Co-op options" src="https://github.com/user-attachments/assets/c6583d26-e395-44f4-b4a0-91d7f4574709" />

---

## Controller Detection

Hollow Knight may occasionally behave inconsistently when multiple controllers are connected, particularly in menus. This behavior can also occur without GrimmchildCoopMod installed.

GrimmchildCoopMod assigns the Knight and Grimmchild controllers independently once in-game.

If one of the controllers is not detected or assigned correctly, try reconnecting the controllers or restarting the game with both controllers already connected.

---

## Known Limitations

- Two XInput-compatible controllers are required.
- Grimmchild is permanently forced to Level 4, so parts of the Grimm Troupe progression may not behave as originally intended.
- Grimmchild currently uses the original Level 4 sprites.
- Hollow Knight can occasionally behave inconsistently when handling multiple controllers, especially in menus.
- On a brand-new save file, you may need to save, quit, and reload once for Grimmchild to appear correctly.

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
