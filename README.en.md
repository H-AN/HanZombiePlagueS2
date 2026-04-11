<div align="center"><h1><img width="600" height="131" alt="68747470733a2f2f70616e2e73616d7979632e6465762f732f56596d4d5845" src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" /></h1></div>

<div class="section">
<div align="center"><h1>Zombie Plague for Swiftly2</h1></div>


<div align="center"><strong>基于 Swiftly2 框架开发的 CS2 僵尸瘟疫插件。</p></div>

<div align="center"><strong>支持多种自定义配置/Supports multiple custom configurations。</p></div>
<div align="center"><strong>支持自定义丧尸种类,多重游戏模式,僵尸瘟疫道具,支持API拓展,音效系统等。</p></div>
  <div align="center"><strong>supports customizable zombie types, multiple game modes, zombie plague items, sound effects system</p></div>
     <div align="center"><strong>supports API expansion.</p></div>
</div>

<div align="center">

<div style="display:flex; align-items:center; gap:6px;">
  <span>Support yumiai :</span>
  <a href="https://yumi.chat:3000/">
    <img src="https://yumi.chat:3000/logo.png" width="50">
  </a>
  <span>(The best AI model provider, click the icon to visit the official website.)</span>
</div>

---

如果你喜欢这个插件,可以用以下方式支持我,感谢!

If you like this plugin, you can support me in the following ways. Thank you!

[![ko-fi](https://github.com/user-attachments/assets/3c01a28f-efe2-48af-9385-cef3a99fbb8c)](https://www.ifdian.net/a/XMHHAN)
[![paypal](https://github.com/user-attachments/assets/da293573-12c8-40bc-b956-d562cd46d4ae)](https://www.paypal.com/paypalme/XMHHAN)
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)

---

## video
https://www.youtube.com/watch?v=DVeR5u28M_s

If you encounter client crashes, you can use https://github.com/H-AN/HZPFixes to reduce the probability of crashes.
</div>

---

Example Workshop files
```
sound : 3644652779
zombie models : ❗Due to copyright restrictions, model examples are no longer provided.

This plugin is compatible with any character model; please find your own model to use!
```

---

<div align="center">
  <a href="./README.en.md"><img src="https://flagcdn.com/48x36/gb.png" alt="English" width="48" height="36" /> <strong>English</strong></a>  
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="./README.md"><img src="https://flagcdn.com/48x36/cn.png" alt="中文" width="48" height="36" /> <strong>中文版</strong></a>
</div>

<hr>

# Han Zombie Plague S2

**Zombie Plague Plugin**  
A Zombie Plague mode plugin for Counter-Strike 2. Featuring rich game modes, special zombie/human classes, prop systems, full API support — bring endless fun and challenge to your server!

## Feature Overview

- **10 Diverse Game Modes**: From classic infection to special class confrontations, all fully configurable.
- **Special Class System**: Mother Zombie, Nemesis, Assassin, Survivor, Sniper, Hero, etc. Each class has independent attributes (health, speed, gravity, damage, model, weapon).
- **Props & Abilities**: T-Virus Grenade (infection area), Incendiary Grenade, Flashbang, Freeze Grenade, Teleport Grenade, SCBA Suit (infection immunity), God Mode, Infinite Ammo, Infinite Clip, No Recoil.
- **Custom Configuration**: Per-mode toggle for infinite ammo & zombie respawn; global settings for knockback force, spawn points, sound effects, ambient music, etc.
- **Player Interaction**: Menu-based zombie class preference selection (saved to database), admin menu, kill damage HUD display.
- **API Support**: Complete event system (infection, class selection, victory, etc.) for other plugins to extend custom logic.
- **Sound & Visuals**: Dedicated sound effects (infection, prop usage), player glow outline, FOV adjustment, looping ambient atmosphere sounds.
- **Balance Optimization**: Knockback system with separate multipliers (head/body/ground/air), independent hero knockback config.

## Game Modes

The plugin provides **10 classic and innovative modes**, each with independent configuration:
- **Zombie Respawn Toggle** (`ZombieCanReborn`)
- **Human Infinite Ammo** (`EnableInfiniteClipMode`)
- **Mode Weight** (`Weight`, used for random mode selection)

1. **Normal Infection Mode**  
   Select 1 player (configurable count) as Mother Zombie to start infecting humans. Classic progressive spread gameplay.

2. **Multi Infection Mode**  
   Select half the players as Mother Zombies to start infecting simultaneously. Fast entry into high-intensity chaos.

3. **Survivor Mode**  
   Select 1 human as Survivor, equipped with M249 machine gun + special attributes (high health, speed, low gravity, high damage — all configurable). All others become zombies for a lone survival battle.

4. **Sniper Mode**  
   Select 1 human as Sniper, equipped with sniper rifle (AWP) + special attributes (high health, speed, low gravity, high damage — configurable). All others become zombies for precision shooting vs horde.

5. **Swarm Mode (Confrontation Mode)**  
   Half the players instantly become zombies (no infection phase) for direct human vs zombie firepower battle.

6. **Plague Mode**  
   Half the players become zombies + 1 Survivor + 1 Nemesis for epic multi-faction chaos battle.

7. **Assassin Mode**  
   Select 1 zombie as Assassin (invisible at long range, visible when close or attacked — invisibility distance configurable). No infection — stealth assassination gameplay.

8. **Nemesis Mode**  
   Select 1 zombie as Nemesis (high-stat boss). No infection — ultimate single boss vs all humans.

9. **Hero Mode**  
   The last x surviving humans (count configurable) automatically become Heroes with ultra-strong attributes and continue the fight.

10. **Sniper vs Assassin Mode**  
    Half the players become zombies + 1 Sniper + 1 Assassin — intense three-way faction battle.

## Configuration Guide

Main configuration file: `HZPMainCFG.json`.

## Global Configuration

Main configuration file: `HZPMainCFG.json`.

The following table lists the key global settings that apply to all modes (unless overridden by mode-specific options).

| Parameter                  | Description                                      | Example Value                                      |
|----------------------------|--------------------------------------------------|----------------------------------------------------|
| `RoundReadyTime`           | Round preparation time (seconds)                 | 22.0                                               |
| `RoundTime`                | Round duration (minutes)                         | 4.0                                                |
| `HumandefaultModel`        | Default human model path                         | "characters/models/ctm_st6/ctm_st6_variante.vmdl" |
| `HumanMaxHealth`           | Maximum health for humans                        | 225                                                |
| `HumanInitialSpeed`        | Initial movement speed for humans (multiplier)   | 1.0                                                |
| `HumanInitialGravity`      | Initial gravity scale for humans                 | 0.8                                                |
| `EnableDamageHud`          | Show kill damage HUD                             | true                                               |
| `EnableInfiniteReserveAmmo`| Infinite reserve ammo for humans                 | true                                               |
| `EnableWeaponNoRecoil`     | Weapons have no recoil                           | true                                               |
| `HumanSpawnPoints`         | Human spawn points (CT/T/DM)                     | "CT,T,DM"                                          |
| `ZombieSpawnPoints`        | Zombie spawn points (CT/T/DM)                    | "CT,T,DM"                                          |
| `KnockZombieForce`         | Knockback force applied to zombies               | 250.0                                              |
| `StunZombieTime`           | Stun duration when knocking back zombies (seconds) | 0.1                                              |
| `AmbSound`                 | List of ambient atmosphere sounds (comma-separated) | "han.zombie.amb.zriot,..."                       |
| `AmbSoundLoopTime`         | Ambient sound loop interval (seconds)            | 60.0                                               |
| `AmbSoundVolume`           | Ambient sound volume                             | 0.8                                                |

### Knockback System

The knockback system allows customizable force when humans shoot zombies, helping balance gameplay and prevent zombies from rushing too easily.

- `HumanKnockBackHeadMultiply`: Headshot knockback multiplier (2.0)  
- `HumanKnockBackBodyMultiply`: Body shot knockback multiplier (1.0)  
- `HumanKnockBackGroundMultiply`: Ground knockback multiplier (1.0)  
- `HumanKnockBackAirMultiply`: Airborne knockback multiplier (0.5)  
- `HumanHeroKnockBackMultiply`: Knockback multiplier when the shooter is a Hero (1.0)

### Props Configuration

| Prop              | Toggle Parameter          | Auto-Give on Spawn Parameter | Range / Duration     | Damage / Effect                  | Sound Effect                          |
|-------------------|---------------------------|------------------------------|----------------------|----------------------------------|---------------------------------------|
| T-Virus Grenade   | -                         | -                            | 300.0               | Can infect Heroes                | "han.zombieplague.grenadedote"       |
| Incendiary Grenade| `FireGrenade`             | `SpawnGiveFireGrenade`       | 300.0 / 5.0s        | 500 initial + 10/s burning       | "han.zombieplague.grenadedote"       |
| Incendiary Bomb   | -                         | `SpawnGiveIncGrenade`        | -                   | Burning damage                   | -                                     |
| Flashbang / Light Grenade | `LightGrenade`    | `SpawnGiveLightGrenade`      | 1000.0 / 30.0s      | Blinding / strong light effect   | "C4.ExplodeTriggerTrip"               |
| Freeze Grenade    | `FreezeGrenade`           | `SpawnGiveFreezeGrenade`     | 300.0 / 10.0s       | Freezes target                   | "han.zombieplague.grenadedote"       |
| Teleport Grenade  | `TelportGrenade`          | `SpawnGiveTelportGrenade`    | -                   | Teleports player                 | -                                     |
| T-Virus Serum     | -                         | -                            | -                   | Turns zombie back to human (special zombies immune) | "HealthShot.Pickup"                   |
| SCBA Suit (Chemical Suit) | `CanUseScbaSuit`  | -                            | -                   | Immune to infection              | Pickup: "Player.PickupPistol"<br>Broken: "Breakable.Flesh" |

### Mode Configuration Examples

Each mode has its own independent settings:
- `Enable`: Toggle the mode on/off
- `Name`: Display name in-game
- `Weight`: Random selection weight (higher = more likely to be chosen)

Specific per-mode parameters:
- **Normal Infection**: `MotherZombieCount` (number of Mother Zombies, default 1)
- **Survivor**: `SurvivorHealth` (1000), `SurvivorSpeed` (3.0), custom model/weapon paths
- **Sniper**: `SniperHealth` (500), `SniperWeapon` ("weapon_awp")
- **Assassin**: `InvisibilityDist` (invisibility distance, 200.0)
- **Hero**: `HeroCount` (number of Heroes, e.g. 3)

For the full JSON configuration, see the `configs/` folder in the repository.

## Installation Guide

1. Download the plugin package and extract it to `addons/swiftlys2/plugins/`.
2. Start (or restart) the server.
3. Edit and configure `HZPMainCFG.json` (and other config files as needed).
4. Ensure dependencies: The plugin requires the SwiftlyS2 framework to be installed and running.

After installation, load the map or use the command to reload the plugin if necessary. Check server console/logs for any loading errors.

## Commands List

- `!zclass` or `sw_zclass`: Opens the zombie class selection menu (player preference, command can be freely customized in config).
- `!zmenu` or `sw_zmenu`: Opens the admin menu (requires permission `AdminMenuPermission`; if left empty, accessible to everyone).

---

## Zombie Class Configuration

The plugin supports a rich zombie class system, divided into two separate configuration files:

- **HZPZombieClassCFG.json**: List of normal zombie classes (`ZombieClassList`).  
  These are the standard zombie types players may become after normal infection (e.g., Red Skull, White Skull, Xenomorph Queen, etc.).

- **HZPSpecialClassCFG.json**: List of special zombie classes (`SpecialClassList`).  
  These are the special roles used in specific modes (e.g., Mother Zombie, Nemesis, Assassin, etc.).

**Both files use exactly the same format** — they are separated only to distinguish between normal zombies and special (mode-specific) zombies.

Each zombie class follows this structure:
Each zombie class follows this structure:

```json
{
  "Name": "Class Name",          // Must be unique, used for matching in main config
  "Enable": true,                // Whether this class is enabled
  "PrecacheSoundEvent": "...",   // Path to pre-cache sound event file
  "Stats": { ... },              // Numerical stats / attributes
  "Models": { ... },             // Model paths
  "Sounds": { ... }              // Various sound effects
}
```
---

Main Config Matching Mechanism for Zombie Classes
In the main configuration file HZPMainCFG.json, special modes match zombie classes by name fields. Example:
```
"Nemesis": {
  "Enable": true,
  "Name": "Nemesis Mode",
  "NemesisNames": "Nemesis",   // Must exactly match the "Name" in HZPSpecialClassCFG.json
  "Weight": 50,
  ...
}
```
Fields like NemesisNames, AssassinNames, SurvivorNames, etc., must exactly match the "Name" value in the corresponding config file (HZPSpecialClassCFG.json or HZPZombieClassCFG.json).
If the name does not match, the class is disabled (Enable: false), or the class does not exist, the plugin will fail to load that role, which may cause mode errors or fallback to default behavior.

---

### Stats (Attributes) Parameters

| Parameter           | Description                                      | Example Value | Notes                                      |
|---------------------|--------------------------------------------------|---------------|--------------------------------------------|
| Health              | Maximum health in normal state                   | 8000         | -                                          |
| MotherZombieHealth  | Maximum health when acting as Mother Zombie      | 18000        | Only applies in Mother Zombie modes        |
| Speed               | Movement speed multiplier (1.0 = default human speed) | 1.0 ~ 2.5 | Higher value = faster movement             |
| Damage              | Base melee attack damage                         | 50.0         | Claw/knife damage                          |
| Gravity             | Gravity scale (lower value = higher jumps, slower fall) | 0.7    | Typically 0.2 ~ 1.0                        |
| Fov                 | Field of View (FOV)                              | 110          | Wider view for zombies                     |
| EnableRegen         | Enable automatic health regeneration             | true         | -                                          |
| HpRegenSec          | Health regeneration interval (seconds)           | 5.0          | -                                          |
| HpRegenHp           | Health restored per regeneration tick            | 30           | -                                          |
| ZombieSoundVolume   | Volume of zombie-related sounds                  | 1.0          | Range: 0.0 ~ 1.0                           |
| IdleInterval        | Interval between idle sounds (seconds)           | 70.0         | -                                          |
|-----------------------|-------------------------------|-----------------------------------------------------------------------------|
| Parameter             | Description                              | Example Path Example                                                  |
|-----------------------|------------------------------------------|-----------------------------------------------------------------------|
| ModelPath             | Main zombie model path                   | characters/models/voikanaa/feral_ghoul_fonv/feral_ghoul_fonv.vmdl   |
| CustomKinfeModelPath  | Custom claw/knife model path (optional)  | "" (uses default)                                                     |
| Parameter     | Description                          | Example Sound Key(s)                          |
|---------------|--------------------------------------|-----------------------------------------------|
| SoundInfect   | Sound played when infected           | han.human.mandeath                            |
| SoundPain     | Pain/injury sound                    | han.hl.zombie.pain                            |
| SoundHurt     | Hurt sound                           | han.zombie.manclassic_hurt                    |
| SoundDeath    | Death sound                          | han.zombie.manclassic_death                   |
| IdleSound     | Idle/breathing sound (multiple allowed, comma-separated) | han.hl.nihilanth.idle,han.hl.nihilanth.idleb |
| RegenSound    | Health regeneration sound            | han.zombie.state.manheal                      |
| BurnSound     | Sound when burning                   | han.zombieplague.zburn                        |
| ExplodeSound  | Explosion/special death sound        | han.hl.zombie.idle                            |
| HitSound      | Sound when hitting an enemy          | han.zombie.classic_hit                        |
| HitWallSound  | Sound when hitting a wall            | han.zombie.classic_hitwall                    |
| SwingSound    | Sound when swinging claw/missing attack | han.zombie.classic_swing                   |
| Class Name          | Normal Health | Mother Health | Speed | Gravity | FOV | Regen Interval / Amount | Special Notes                          |
|---------------------|---------------|---------------|-------|---------|-----|-------------------------|----------------------------------------|
| Red Skull           | 8000         | 18000        | 1.0  | 0.7    | 110 | 5.0s / 30              | High durability, auto-regen            |
| White Skull         | 3000         | 13000        | 1.1  | 0.8    | 110 | 5.0s / 30              | Medium health, slightly faster speed   |
| frozen              | 5000         | 15000        | 1.7  | 0.7    | 110 | 1.0s / 150             | High regen rate (disabled)             |
| Fat Guy             | 5000         | 15000        | 1.7  | 0.8    | 110 | 1.0s / 150             | High regen rate (disabled)             |
| Xenomorph Queen     | 2500         | 12500        | 2.0  | 0.2    | 110 | 10.0s / 5              | Extremely low gravity, high speed, female sounds |
| Female Scientist Zombie | 1800     | 12000        | 1.8  | 0.5    | 110 | 10.0s / 5              | High speed, female sounds              |
|---------------------|---------------|---------------|-------|---------|-----|-------------------------|----------------------------------------|
| Class Name     | Normal Health | Mother Health | Speed | Gravity | FOV | Regen Interval / Amount | Special Notes                              |
|----------------|---------------|---------------|-------|---------|-----|-------------------------|--------------------------------------------|
| Mother Zombie  | 15000        | 20000        | 1.5  | 0.5    | 110 | 1.0s / 50              | Initial infection source, high damage (150) |
| Nemesis        | 30000        | 50000        | 2.0  | 0.3    | 120 | 1.0s / 50              | Ultimate boss, ultra-high health, low gravity |
| Assassin       | 15000        | 35000        | 2.5  | 0.4    | 120 | 2.0s / 60              | Ultra-high speed, pairs with invisibility mechanic |
---

## Sound Broadcast System (Vox System)

The plugin includes a powerful sound broadcast system (Vox) that automatically plays voice announcements at key game moments (such as round start, countdown, mode announcement, victory declaration, etc.), greatly enhancing immersion and atmosphere.

Configuration file: **HZPVoxCFG.json**

### Vox System Structure

`VoxList` is an array, where each element represents a complete voice broadcast package (e.g., CSOL Male, CSOL Female, HL1 Male, etc.).

Each voice package follows this structure:

```json
{
  "Name": "Voice Package Name",          // Only for display and identification
  "Enable": true,                        // Whether this voice package is enabled
  "PrecacheSoundEvent": "...",           // Path to pre-cache sound event file (required)
  "RoundMusicVox": "...",                // Round start music/voice (multiple allowed, comma-separated)
  "SecRemainVox": "...",                 // 20 seconds remaining reminder voice
  "CoundDownVox": "...",                 // Countdown voice (10~1 seconds, multiple allowed)
  "ZombieSpawnVox": "...",               // Zombie spawn/appearance voice
  "NormalInfectionVox": "...",           // Normal Infection mode announcement voice
  "MultiInfectionVox": "...",            // Multi Infection mode announcement voice
  "NemesisVox": "...",                   // Nemesis mode announcement voice
  "SurvivorVox": "...",                  // Survivor mode announcement voice
  "SwarmVox": "...",                     // Swarm mode announcement voice
  "PlagueVox": "...",                    // Plague mode announcement voice
  "AssassinVox": "...",                  // Assassin mode announcement voice
  "SniperVox": "...",                    // Sniper mode announcement voice
  "AVSVox": "...",                       // Sniper vs Assassin mode announcement voice
  "HeroVox": "...",                      // Hero mode announcement voice
  "HumanWinVox": "...",                  // Human victory voice (multiple allowed)
  "ZombieWinVox": "..."                  // Zombie victory voice (multiple allowed)
}
```

### Vox Parameters / Voice Trigger Events

| Parameter          | Trigger Timing                          | Example Sound Key(s) (multiple allowed, comma-separated) | Notes                                      |
|--------------------|-----------------------------------------|-----------------------------------------------------------|--------------------------------------------|
| RoundMusicVox      | Round officially starts (prep time ends) | han.zombie.round.class_start                             | Often used for background music or opening voice |
| SecRemainVox       | 20 seconds remaining in round           | han.zombie.round.20secremain                             | Reminds players time is running out        |
| CoundDownVox       | Countdown 10~1 seconds (one per second) | han.zombie.round.mancdone,...                            | Supports 10 separate voices, played in sequence |
| ZombieSpawnVox     | Zombie spawn or Mother Zombie appears   | han.zombie.round.manzbcome                               | Builds tension                             |
| NormalInfectionVox | Normal Infection mode announcement      | han.zombieplague.end.horror                              | Mode-specific voice                        |
| MultiInfectionVox  | Multi Infection mode announcement       | han.zombieplague.end.horror                              | -                                          |
| NemesisVox         | Nemesis mode announcement               | han.zombieplague.type.nemesis                            | -                                          |
| SurvivorVox        | Survivor mode announcement              | han.zombieplague.type.survivor                           | -                                          |
| SwarmVox           | Swarm / Legion mode announcement        | han.zombieplague.end.horror                              | -                                          |
| PlagueVox          | Plague mode announcement                | han.zombieplague.end.plague                              | -                                          |
| AssassinVox        | Assassin mode announcement              | han.zombieplague.type.nemesis                            | -                                          |
| SniperVox          | Sniper mode announcement                | han.zombieplague.type.survivor                           | -                                          |
| AVSVox             | Sniper vs Assassin mode announcement    | han.zombieplague.type.nemesis                            | -                                          |
| HeroVox            | Hero mode announcement                  | han.zombieplague.type.survivor                           | -                                          |
| HumanWinVox        | Humans win                              | han.zombie.round.manhmwin                                | Multiple allowed — random playback         |
| ZombieWinVox       | Zombies win                             | han.zombie.round.manzbwin                                | Multiple allowed — random playback         |
### Voice Package Examples (HZPVoxCFG.json)

| Voice Package Name   | Style Source          | Enabled | Special Features                                                                 |
|----------------------|-----------------------|---------|----------------------------------------------------------------------------------|
| CSOL Male Broadcast  | CSOL Male Style       | true   | Classic male announcer, clear and powerful countdown, exciting victory voice   |
| CSOL Female Broadcast| CSOL Female Style     | true   | Female voice, gentle yet tense countdown, suitable for varied atmospheres       |
| HL1 Male Broadcast   | Half-Life 1 Male      | true   | Retro HL1 style, classic infection sounds and victory announcements             |
| HL1 Female Broadcast | Half-Life 1 Female    | true   | HL1 female voice, unique retro atmosphere, special remaining time reminders     |
| Zombie Plague Broadcast | Zombie Plague Classic | true   | Mix of various classic voices, rich victory announcements, strong random playback effect |

**Customization Tips**:
- Each voice package can be independently enabled or disabled (`"Enable": true/false`).
- For the same event, you can list multiple sound keys (comma-separated) — the system will **randomly play** one of them for variety.
- All voices must be pre-cached in the specified `PrecacheSoundEvent` file.
- You can add your own custom voice packages (e.g., Japanese, Korean, or localized voices) as long as the sound file paths are correct.

**Recommended Usage**:
- Servers can switch voice packages based on event themes (e.g., horror style for Halloween, cheerful for holidays).
- Combining different voice packages with modes creates stronger thematic immersion (e.g., HL1 package with retro maps, CSOL package with high-intensity matches).

---


## API Support

Full API interface (`IHanZombiePlagueAPI`) is provided, supporting event listening (e.g., infection, victory), player status queries, forced class setting, and more.  
See details in [HanZombiePlagueAPI.xml](API/net10.0/HanZombiePlagueAPI.xml)

This API allows other plugins to:
- Listen to key events (e.g., `HZP_OnPlayerInfect`, `HZP_OnNemesisSelected`, `HZP_OnGameStart`, `HZP_OnHumanWin`, etc.)
- Query player states (is zombie? is Nemesis? current mode? etc.)
- Forcefully set player roles/classes (e.g., make someone a Survivor, Nemesis, or Hero)
- Interact with game flow (check win conditions, give props, set glow/FOV/god mode, etc.)

For full documentation, including all methods, events, and parameters, refer to the API header file in the repository.






