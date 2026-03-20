using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Cecil.Cil;
using Spectre.Console;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Helpers;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Sounds;
using static Dapper.SqlMapper;
using static HanZombiePlagueS2.HZPZombieClassCFG;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace HanZombiePlagueS2;

public partial class HZPEvents
{
    private readonly ILogger<HZPEvents> _logger;
    private readonly ISwiftlyCore _core;
    private readonly HZPGlobals _globals;
    private readonly HZPServices _service;
    private readonly HZPCommands _commands;
    private readonly HZPHelpers _helpers;
    private readonly IOptionsMonitor<HZPMainCFG> _mainCFG;
    private readonly IOptionsMonitor<HZPVoxCFG> _voxCFG;
    private readonly IOptionsMonitor<HZPZombieClassCFG> _zombieClassCFG;
    private readonly IOptionsMonitor<HZPSpecialClassCFG> _SpecialClassCFG;
    private readonly PlayerZombieState _zombieState;
    private readonly HZPGameMode _gameMode;

    private readonly HanZombiePlagueAPI _api;
    public HZPEvents(ISwiftlyCore core, ILogger<HZPEvents> logger
        , HZPGlobals globals, HZPServices services,
        HZPCommands commands, IOptionsMonitor<HZPMainCFG> mainCFG,
        IOptionsMonitor<HZPVoxCFG> voxCFG, HZPHelpers helpers, 
        IOptionsMonitor<HZPZombieClassCFG> zombieClassCFG,
        PlayerZombieState zombieState, HZPGameMode gameMode,
        IOptionsMonitor<HZPSpecialClassCFG> specialClassCFG,
        HanZombiePlagueAPI api)
    {
        _core = core;
        _logger = logger;
        _globals = globals;
        _service = services;
        _commands = commands;
        _mainCFG = mainCFG;
        _voxCFG = voxCFG;
        _helpers = helpers;
        _zombieClassCFG = zombieClassCFG;
        _zombieState = zombieState;
        _gameMode = gameMode;
        _SpecialClassCFG = specialClassCFG;
        _api = api;
    }

    public void HookEvents()
    {
        _core.GameEvent.HookPre<EventRoundStart>(OnTimerStart);
        _core.GameEvent.HookPre<EventRoundFreezeEnd>(OnRoundFreezeEnd);
        _core.GameEvent.HookPre<EventRoundEnd>(OnRoundEnd);
        _core.GameEvent.HookPre<EventPlayerDeath>(OnPlayerDeath);
        
        _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurtInfect);
        _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurtZombie);
        _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerDmgHud);


        _core.Event.OnClientDisconnected += Event_OnClientDisconnected;
        _core.Event.OnClientConnected += Event_OnClientConnected;
        _core.Event.OnEntityTakeDamage += Event_OnEntityTakeDamage;
        _core.Event.OnMapLoad += Event_OnMapLoad;
        _core.Event.OnWeaponServicesCanUseHook += Event_OnWeaponServicesCanUseHook;
        _core.Event.OnPrecacheResource += Event_OnPrecacheResource;
        _core.Event.OnTick += Event_OnTickSpeed;
        _core.Event.OnTick += Event_OnTickNoRecoil;

        _core.GameEvent.HookPre<EventWeaponFire>(OnHumanWeaponFire);
        _core.Event.OnEntityTakeDamage += Event_OnHumanTakeDamage;

        _core.GameEvent.HookPre<EventPlayerDeath>(CheckRoundWinDeath);

        _core.GameEvent.HookPre<EventPlayerSpawn>(OnPlayerSpawn);
        _core.GameEvent.HookPre<EventPlayerSpawn>(CheckRoundWinSpawn);
        _core.GameEvent.HookPre<EventPlayerSpawn>(RandomSpawn);


        _core.GameEvent.HookPre<EventGrenadeThrown>(OnGrenadeThrown);
        _core.GameEvent.HookPre<EventHegrenadeDetonate>(OnGrenadeDetonate);

        _core.GameEvent.HookPre<EventPlayerBlind>(OnPlayerBlind);
        _core.GameEvent.HookPre<EventFlashbangDetonate>(OnFlashbangDetonate);

        _core.GameEvent.HookPre<EventSmokegrenadeDetonate>(OnSmokegrenadeDetonate);

        _core.GameEvent.HookPre<EventDecoyFiring>(OnDecoyFiring);

        _core.Event.OnEntityCreated += Event_OnEntityCreated;
    }

    private void Event_OnEntityCreated(IOnEntityCreatedEvent @event)
    {
        var entity = @event.Entity;
        if (entity == null || !entity.IsValid || !entity.IsValidEntity)
            return;

        if (!entity.DesignerName.Contains("_projectile"))
            return;

        _core.Scheduler.NextTick(() =>
        {
            if (entity.IsValid && entity.IsValidEntity)
            {
                _helpers.CheckGrenadeSpawned(entity);
            }
        });
    }

    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event)
    {

        try
        {
            if (!_globals.SafeRoundStart)
                return HookResult.Continue;

            _globals.SafeRoundStart = false;

            _helpers.SwitchAllPlayerTeam();
            _commands.RoundCvar();
            _helpers.BuildSpawnCache();
            _helpers.RemoveHostage();

            var playerCount = _helpers.ServerPlayerCount();
            if (playerCount <= 0)
            {
                _globals.ServerIsEmpty = true;
                return HookResult.Continue;
            }
            _globals.ServerIsEmpty = false;

            var CFG = _mainCFG.CurrentValue;
            var VoxCFG = _voxCFG.CurrentValue;
            var VoxList = VoxCFG.VoxList;

            _helpers.SetAllDefaultModel(CFG);

            //_logger.LogInformation("开始选择游戏模式...");
            var selectedMode = _gameMode.PickRandomMode();
            //_logger.LogInformation($"当前模式: {_gameMode.GetModeName()}");

            if (_api != null)
                _api.NotifyGameModeSelect(_gameMode.GetModeName());

            _globals.IsheroSetup = false;
            _globals.GameInfiniteClipMode = false;
            _service.CheckEndTimer();
            if (_globals.RoundVoxGroup == null && VoxList != null)
            {
                _globals.RoundVoxGroup = _helpers.PickRandomActiveGroup(VoxList);
            }

            if (CFG.RoundReadyTime > 0)
            {
                //_logger.LogInformation($"开始倒计时: {CFG.RoundReadyTime}秒");
                _globals.Countdown = (int)Math.Ceiling(CFG.RoundReadyTime);

                if (_globals.GameStart)
                    return HookResult.Continue;

                if (_globals.RoundVoxGroup != null)
                {
                    //_logger.LogInformation($"播放背景音乐: {_globals.RoundVoxGroup.RoundMusicVox}");
                    _service.PlayerSelectSoundtoAll(_globals.RoundVoxGroup.RoundMusicVox, _globals.RoundVoxGroup.Volume);
                }

                _globals.g_hCountdown?.Cancel();
                _globals.g_hCountdown = null;
                _globals.g_hCountdown = _core.Scheduler.DelayAndRepeatBySeconds(0.1f, 1.0f, () => _service.Round_Countdown());
                _core.Scheduler.StopOnMapChange(_globals.g_hCountdown);

            }
            else
            {
                _globals.Countdown = 3;

                if (_globals.GameStart)
                    return HookResult.Continue;

                if (_globals.RoundVoxGroup != null)
                {
                    //_logger.LogInformation($"播放背景音乐: {_globals.RoundVoxGroup.RoundMusicVox}");
                    _service.PlayerSelectSoundtoAll(_globals.RoundVoxGroup.RoundMusicVox, _globals.RoundVoxGroup.Volume);
                }

                _globals.g_hCountdown?.Cancel();
                _globals.g_hCountdown = null;
                _globals.g_hCountdown = _core.Scheduler.DelayAndRepeatBySeconds(0.1f, 1.0f, () => _service.Round_Countdown());
                _core.Scheduler.StopOnMapChange(_globals.g_hCountdown);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"OnRoundStart ERROR: {ex.Message}");
            _logger.LogError($"ERROR: {ex.StackTrace}");

            if (_globals.RoundVoxGroup != null)
            {
                _logger.LogError($"RoundMusicVox: {_globals.RoundVoxGroup.RoundMusicVox}");
                _logger.LogError($"Volume: {_globals.RoundVoxGroup.Volume}");
            }

            return HookResult.Continue;
        }

        return HookResult.Continue;
    }

    private HookResult OnTimerStart(EventRoundStart @event)
    {
        _service.SetRoundEndTime();
        _globals.SafeRoundStart = true;
        var CFG = _mainCFG.CurrentValue;
        float configDist = CFG.Assassin.InvisibilityDist;
        _core.Scheduler.DelayBySeconds(1.0f, () =>
        {
            _service.GlobalIdleTimer();
            _service.ZombieRegenTimer();
            _service.StartAssassinInvisibilityTimer(configDist);
        });
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event)
    {
        
        _helpers.ClearAllBurns();
        _helpers.ClearAllLights();
        _globals.GameInfiniteClipMode = false;
        _core.Scheduler.DelayBySeconds(2.0f, () =>
        {
            _globals.RoundVoxGroup = null;
            _globals.GameStart = false;
            _globals.g_hRoundEndTimer?.Cancel();
            _globals.g_hRoundEndTimer = null;
            var allplayer = _core.PlayerManager.GetAllPlayers();
            foreach (var player in allplayer)
            {
                if (player == null || !player.IsValid)
                    continue;

                _helpers.RemoveGlow(player);

                var id = player.PlayerID;
                _globals.IsZombie[id] = false;
                _globals.IsSurvivor[id] = false;
                _globals.IsAssassin[id] = false;
                _globals.IsSniper[id] = false;
                _globals.IsNemesis[id] = false;
                _globals.IsHero[id] = false;

                _globals.ScbaSuit[id] = false;
                _globals.GodState[id] = false;
                _globals.InfiniteAmmoState[id] = false;

                _service.StopAssassinTimer();
                _helpers.SetUnInvisibility(player);

                player.SwitchTeam(Team.CT);
                _helpers.ChangeKnife(player, false, false);
                _helpers.SetFov(player, 90);

            }
        });
        
        return HookResult.Continue;
    }
    private void Event_OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        @event.AddItem("characters/models/ctm_st6/ctm_st6_variante.vmdl");
        @event.AddItem("particles/burning_fx/env_fire_large.vpcf");
        @event.AddItem("soundevents/game_sounds_physics.vsndevts");
        @event.AddItem("soundevents/game_sounds_weapons.vsndevts");
        @event.AddItem("soundevents/game_sounds_player.vsndevts");

        @event.AddItem("particles/ui/hud/ui_map_def_utility_trail.vpcf");
        @event.AddItem("particles/burning_fx/barrel_burning_trail.vpcf");
        @event.AddItem("particles/environment/de_train/train_coal_dump_trails.vpcf");

        @event.AddItem("particles/explosions_fx/explosion_hegrenade_water_intial_trail.vpcf");
        @event.AddItem("particles/survival_fx/danger_trail_spores_world.vpcf");

        var CFG = _mainCFG.CurrentValue;
        var ambsound = CFG.PrecacheAmbSound;
        if (!string.IsNullOrEmpty(ambsound))
        {
            var ambsoundList = ambsound
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s));

            foreach (var ambsounds in ambsoundList)
            {
                @event.AddItem(ambsounds);
            }
        }

        var VoxCFG = _voxCFG.CurrentValue;
        var VoxList = VoxCFG.VoxList;
        foreach (var vox in VoxList)
        {
            if (!string.IsNullOrEmpty(vox.PrecacheSoundEvent))
            {
                @event.AddItem(vox.PrecacheSoundEvent);
            }     
        }
        var zombieConfig = _zombieClassCFG.CurrentValue;
        var zombieList = zombieConfig.ZombieClassList;
        foreach (var sounds in zombieList)
        {
            if (!string.IsNullOrEmpty(sounds.PrecacheSoundEvent))
            {
                var soundList = sounds.PrecacheSoundEvent
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));

                foreach (var sound in soundList)
                {
                    @event.AddItem(sound);
                }
            }
        }
        foreach (var models in zombieList)
        {
            if (!string.IsNullOrEmpty(models.Models.ModelPath))
            {
                @event.AddItem(models.Models.ModelPath);
            }
            if (!string.IsNullOrEmpty(models.Models.CustomKinfeModelPath))
            {
                @event.AddItem(models.Models.CustomKinfeModelPath);
            }
        }

        var Survivormodel = CFG.Survivor.ModelsPath;
        if (!string.IsNullOrEmpty(Survivormodel))
        {
            @event.AddItem(Survivormodel);
        }
        var Snipermodel = CFG.Sniper.ModelsPath;
        if (!string.IsNullOrEmpty(Snipermodel))
        {
            @event.AddItem(Snipermodel);
        }
        var Heromodel = CFG.Hero.ModelsPath;
        if (!string.IsNullOrEmpty(Heromodel))
        {
            @event.AddItem(Heromodel);
        }

        var HumanDefaultModel = CFG.HumandefaultModel;
        if (!string.IsNullOrEmpty(HumanDefaultModel))
        {
            @event.AddItem(HumanDefaultModel);
        }

        var SpecialzombieConfig = _SpecialClassCFG.CurrentValue;
        var SpecialzombieList = SpecialzombieConfig.SpecialClassList;
        foreach (var Specialsounds in SpecialzombieList)
        {
            if (!string.IsNullOrEmpty(Specialsounds.PrecacheSoundEvent))
            {
                var SpecialsoundList = Specialsounds.PrecacheSoundEvent
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));

                foreach (var Specialsound in SpecialsoundList)
                {
                    @event.AddItem(Specialsound);
                }
            }
        }
        foreach (var Specialmodels in SpecialzombieList)
        {
            if (!string.IsNullOrEmpty(Specialmodels.Models.ModelPath))
            {
                @event.AddItem(Specialmodels.Models.ModelPath);
            }
            if (!string.IsNullOrEmpty(Specialmodels.Models.CustomKinfeModelPath))
            {
                @event.AddItem(Specialmodels.Models.CustomKinfeModelPath);
            }
        }


    }
    
    private HookResult OnPlayerSpawn(EventPlayerSpawn @event)
    {
        try
        {
            var player = @event.UserIdPlayer;
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            var pawn = @event.UserIdPawn;
            if (pawn == null || !pawn.IsValid)
                return HookResult.Continue;

            var controller = @event.UserIdController;
            if (controller == null || !controller.IsValid)
                return HookResult.Continue;

            var Id = player.PlayerID;
            ulong steamId = player.SteamID;

            _core.Scheduler.NextWorldUpdate(() =>
            {
                try
                {
                    if (player == null || !player.IsValid)
                        return;

                    _helpers.SetNoBlock(player);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"SetNoBlock Error [{controller.PlayerName}]: {ex.Message}");
                }
            });

            _globals.IsZombie.TryGetValue(Id, out bool IsZombie);

            if (IsZombie)
            {
                _core.Scheduler.NextWorldUpdate(() =>
                {
                    try
                    {
                        //_logger.LogInformation($"玩家 [{controller.PlayerName}] 开始应用僵尸类...");

                        var zombieConfig = _zombieClassCFG.CurrentValue;
                        var zombieClasses = zombieConfig.ZombieClassList;
                        var specialConfig = _SpecialClassCFG.CurrentValue;

                        var preference = _zombieState.GetPlayerPreference(Id, steamId);

                        ZombieClass? zombie = null;

                        if (preference != null)
                        {
                            if (preference.Preference == ZombiePreference.Fixed)
                            {
                                zombie = zombieClasses.FirstOrDefault(c => c.Name == preference.FixedZombieName);
                                //_logger.LogInformation($"固定僵尸类: {zombie?.Name}");
                            }
                            else
                            {
                                zombie = _zombieState.PickRandomZombieClass(zombieClasses);
                                //_logger.LogInformation($"随机僵尸类: {zombie?.Name}");
                            }
                        }
                        else
                        {
                            zombie = _zombieState.GetZombieClass(Id, zombieConfig.ZombieClassList, specialConfig.SpecialClassList);
                            if (zombie == null)
                            {
                                zombie = _zombieState.PickRandomZombieClass(zombieClasses);
                                //_logger.LogInformation($"备用随机僵尸类: {zombie?.Name}");
                            }
                        }

                        if (zombie != null)
                        {
                            //_logger.LogInformation($"调用 posszombie: {zombie.Name}, 模型: {zombie.Models}");
                            _service.posszombie(player, zombie, false);
                            //_logger.LogInformation($"posszombie 完成");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"OnPlayerSpawn zombie Class Error [{controller.PlayerName}]: {ex.Message}");
                        _logger.LogError($"Error: {ex.StackTrace}");
                    }
                });
            }
            else
            {
                var CFG = _mainCFG.CurrentValue;
                _core.Scheduler.NextWorldUpdate(() =>
                {
                    pawn.MaxHealth = CFG.HumanMaxHealth;
                    pawn.MaxHealthUpdated();
                    pawn.Health = CFG.HumanMaxHealth;
                    pawn.HealthUpdated();

                    pawn.ActualGravityScale = CFG.HumanInitialGravity;

                    _service.GiveSpawnGrenade(player, CFG);
                });

                
            }

            return HookResult.Continue;
        }
        catch (Exception ex)
        {
            _logger.LogError($"OnPlayerSpawn ERROR: {ex.Message}");
            _logger.LogError($"ERROR: {ex.StackTrace}");
            return HookResult.Continue;
        }
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        var player = @event.UserIdPlayer;
        if(player == null || !player.IsValid)
            return HookResult.Continue;

        var Pawn = player.PlayerPawn;
        if (Pawn == null || !Pawn.IsValid)
            return HookResult.Continue;

        var Controller = player.Controller;
        if (Controller == null || !Controller.IsValid)
            return HookResult.Continue;


        _helpers.SetFov(player, 90);
        _helpers.RemoveGlow(player);

        var Id = player.PlayerID;
        var steamId = player.SteamID;

        _helpers.ClearPlayerBurn(Id);
        _helpers.RemoveSHumanClass(Id);
        _helpers.RemoveSZombieClass(Id);

        _globals.IsMother.Remove(Id);
        _globals.ScbaSuit.Remove(Id);
        _globals.GodState.Remove(Id);
        _globals.InfiniteAmmoState.Remove(Id);




        if (!_globals.GameStart)
            return HookResult.Continue;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie && _gameMode.CanZombieReborn())
        {

            var zombieClasses = _zombieClassCFG.CurrentValue.ZombieClassList;
            var specialClasses = _SpecialClassCFG.CurrentValue.SpecialClassList;
            _core.Scheduler.DelayBySeconds(1.0f, () =>
            {
                _zombieState.ClearSpecialAndSetPlayerZombie(player, zombieClasses, specialClasses);
                player.Respawn();
            });
        }
        if (!IsZombie && _gameMode.CanZombieReborn())
        {
            _core.Scheduler.DelayBySeconds(1.0f, () =>
            {
                var player = _core.PlayerManager.GetPlayer(Id);
                if (player == null || !player.IsValid)
                    return;

                player.Respawn();

                _core.Scheduler.NextWorldUpdate(() =>
                {
                    if (player == null || !player.IsValid)
                        return;

                    var zombieConfig = _zombieClassCFG.CurrentValue;
                    var zombieClasses = zombieConfig.ZombieClassList;
                    var preference = _zombieState.GetPlayerPreference(Id, steamId);
                    ZombieClass? selectedClass;

                    if (preference != null && preference.Preference == ZombiePreference.Fixed)
                    {
                        selectedClass = zombieClasses.FirstOrDefault(c => c.Name == preference.FixedZombieName);
                    }
                    else
                    {
                        selectedClass = _zombieState.PickRandomZombieClass(zombieClasses);
                    }

                    if (selectedClass != null)
                    {
                        _service.posszombie(player, selectedClass, false);
                        _service.PlayerSelectSoundtoAll(selectedClass.Sounds.SoundInfect, selectedClass.Stats.ZombieSoundVolume);
                    }
                });

            });
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerHurtInfect(EventPlayerHurt @event)
    {
        var mode = _gameMode.CurrentMode;
        if (mode != GameModeType.Normal && mode != GameModeType.NormalInfection && mode != GameModeType.MultiInfection
            && mode != GameModeType.Hero)
            return HookResult.Continue;

        var victim = @event.UserIdPlayer;
        if (victim == null || !victim.IsValid)
            return HookResult.Continue;

        var attackerId = @event.Attacker;

        var attacker = _core.PlayerManager.GetPlayer(attackerId);
        if (attacker == null || !attacker.IsValid)
            return HookResult.Continue;

        var vId = victim.PlayerID;
        var aId = attacker.PlayerID;

        var CFG = _mainCFG.CurrentValue;

        int Dmg = @event.DmgHealth;
        int Health = @event.Health;
        string waepon = @event.Weapon;
        _globals.IsZombie.TryGetValue(aId, out bool attackerIsZombie);
        _globals.IsZombie.TryGetValue(vId, out bool victimIsZombie);
        _globals.GodState.TryGetValue(vId, out bool IsGodState);
        if (attackerIsZombie && !victimIsZombie)
        {
            _globals.IsHero.TryGetValue(vId, out bool victimIsIsHero);
            if (victimIsIsHero)
                return HookResult.Continue;

            if (waepon != "knife")
                return HookResult.Continue;

            if(IsGodState)
                return HookResult.Continue;

            _service.Infect(attacker, victim, false);
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerHurtZombie(EventPlayerHurt @event)
    {
        var victim = @event.UserIdPlayer;
        if(victim == null || !victim.IsValid)
            return HookResult.Continue;

        var attackerId = @event.Attacker;

        var attacker = _core.PlayerManager.GetPlayer(attackerId);
        if (attacker == null || !attacker.IsValid)
            return HookResult.Continue;

        var vId = victim.PlayerID;
        var aId = attacker.PlayerID;
        _globals.IsZombie.TryGetValue(aId, out bool attackerIsZombie);
        _globals.IsZombie.TryGetValue(vId, out bool victimIsZombie);
        _globals.IsAssassin.TryGetValue(vId, out bool victimIsAssassin);
        if (!attackerIsZombie && victimIsZombie && victimIsAssassin)
        {
            _helpers.SetUnInvisibility(victim);
            _globals.g_IsInvisible[vId] = false;
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerDmgHud(EventPlayerHurt @event)
    {
        var victim = @event.UserIdPlayer;
        if (victim == null || !victim.IsValid)
            return HookResult.Continue;

        var attackerId = @event.Attacker;

        var attacker = _core.PlayerManager.GetPlayer(attackerId);
        if (attacker == null || !attacker.IsValid)
            return HookResult.Continue;

        var vId = victim.PlayerID;
        var aId = attacker.PlayerID;

        var CFG = _mainCFG.CurrentValue;

        int Dmg = @event.DmgHealth;
        _globals.IsZombie.TryGetValue(aId, out bool attackerIsZombie);
        _globals.IsZombie.TryGetValue(vId, out bool victimIsZombie);

        if (!attackerIsZombie && victimIsZombie && CFG.EnableDamageHud)
        {
            _service.ShowDmgHud(attacker, victim, Dmg);
        }
        return HookResult.Continue;



    }

    public void Event_OnWeaponServicesCanUseHook(IOnWeaponServicesCanUseHookEvent @event)
    {
        var weapon = @event.Weapon;
        var weaponName = weapon?.Entity?.DesignerName; 
        var customname = weapon?.AttributeManager.Item.CustomName;

        var pawn = @event.WeaponServices.Pawn;
        if (pawn == null || !pawn.IsValid) return;

        var controller = pawn.Controller.Value?.As<CCSPlayerController>();
        if (controller == null || !controller.IsValid) return;

        var Player = _core.PlayerManager.GetPlayer((int)(controller.Index - 1));
        if (Player == null || !Player.IsValid) return;

        if (weaponName == "weapon_c4")
        {
            @event.SetResult(false);
            return;
        }

        _globals.IsZombie.TryGetValue(Player.PlayerID, out bool isZombie);
        if (isZombie)
        {
            if (weaponName != "weapon_knife" && customname != "TVirusGrenade")
            {
                @event.SetResult(false);
            }
        }
        else
        {

            bool isGrenade = weaponName == "weapon_hegrenade"
                     || weaponName == "weapon_flashbang"
                     || weaponName == "weapon_decoy"
                     || weaponName == "weapon_incgrenade"
                     || weaponName == "weapon_smokegrenade";

            if (isGrenade)
            {
                var allowedHumanGrenades = new HashSet<string> 
                { 
                    "FireGrenade", 
                    "FreezeGrenade", 
                    "LightGrenade", 
                    "TeleprotGrenade", 
                    "Incgrenade" 
                };

                if (string.IsNullOrEmpty(customname) || !allowedHumanGrenades.Contains(customname))
                {
                    @event.SetResult(false);
                }
            }
        }
    }
    private void Event_OnMapLoad(IOnMapLoadEvent @event)
    {
        _commands.ServerCvar();
        var VoxCFG = _voxCFG.CurrentValue;
        var VoxList = VoxCFG.VoxList;
        if (_globals.RoundVoxGroup == null && VoxList != null)
        {
            _globals.RoundVoxGroup = _helpers.PickRandomActiveGroup(VoxList);
        }
    }
    private void Event_OnEntityTakeDamage(SwiftlyS2.Shared.Events.IOnEntityTakeDamageEvent @event)
    {
        var victim = @event.Entity;
        if (victim == null || !victim.IsValid)
            return;

        var VictimPawn = victim.As<CCSPlayerPawn>();
        if (VictimPawn == null || !VictimPawn.IsValid)
            return;

        var VictimController = VictimPawn.Controller.Value?.As<CCSPlayerController>();
        if (VictimController == null || !VictimController.IsValid)
            return;

        var VictimPlayer = _core.PlayerManager.GetPlayer((int)(VictimController.Index - 1));
        if (VictimPlayer == null || !VictimPlayer.IsValid)
            return;

        var attacker = @event.Info.Attacker.Value;
        if (attacker == null || !attacker.IsValid)
            return;

        var AttackerPawn = attacker.As<CCSPlayerPawn>();
        if (AttackerPawn == null || !AttackerPawn.IsValid)
            return;

        var AttackerController = AttackerPawn.Controller.Value?.As<CCSPlayerController>();
        if (AttackerController == null || !AttackerController.IsValid)
            return;

        var AttackerPlayer = _core.PlayerManager.GetPlayer((int)(AttackerController.Index - 1));
        if (AttackerPlayer == null || !AttackerPlayer.IsValid)
            return;

        var victimId = VictimPlayer.PlayerID;
        var attackerId = AttackerPlayer.PlayerID;

        _globals.IsZombie.TryGetValue(attackerId, out bool attackerIsZombie);
        _globals.IsZombie.TryGetValue(victimId, out bool victimIsZombie);
        _globals.ScbaSuit.TryGetValue(victimId, out bool IsHaveScbaSuit);
        _globals.GodState.TryGetValue(victimId, out bool IsGodState);

        var CFG = _mainCFG.CurrentValue;

        if (!attackerIsZombie && !victimIsZombie)
        {
            @event.Info.Damage = 0;
        }
        else if (attackerIsZombie && !victimIsZombie)
        {
            var zombieConfig = _zombieClassCFG.CurrentValue;
            var specialConfig = _SpecialClassCFG.CurrentValue;
            var zombie = _zombieState.GetZombieClass(attackerId, zombieConfig.ZombieClassList, specialConfig.SpecialClassList);
            if (zombie == null)
                return;

            if (IsHaveScbaSuit)
            {
                @event.Info.Damage = 0;
                _helpers.RemoveScbaSuit(VictimPlayer, CFG.ScbaSuitBrokenSound);
            }
            else if (IsGodState)
            {
                @event.Info.Damage = 0;
            }
            else
            {
                @event.Info.Damage += zombie.Stats.Damage;
            }
        }
        else if (!attackerIsZombie && victimIsZombie)
        {
            if (IsGodState)
            {
                @event.Info.Damage = 0;
            }
        }
    }

    private void Event_OnClientConnected(SwiftlyS2.Shared.Events.IOnClientConnectedEvent @event)
    {
        var id = @event.PlayerId;

        if (_globals.GameStart)
        {
            _service.CheckRoundWinConditions();
        }

        if (_globals.ServerIsEmpty)
        {
            _globals.ServerIsEmpty = false;
            _core.Scheduler.DelayBySeconds(1.0f, () =>
            {
                _helpers.restartgame();
            });
        }

        _globals.IsZombie[id] = _globals.GameStart;

    }

    private void Event_OnClientDisconnected(SwiftlyS2.Shared.Events.IOnClientDisconnectedEvent @event)
    {
        if (_globals.GameStart)
        {
            _service.CheckRoundWinConditions();
        }

        var id = @event.PlayerId;

        _helpers.ClearPlayerBurn(id);
        _globals.IsZombie.Remove(id);
        _globals.IsMother.Remove(id);
        _globals.IsSurvivor.Remove(id);
        _globals.IsSniper.Remove(id);
        _globals.IsNemesis.Remove(id);
        _globals.IsAssassin.Remove(id);
        _globals.IsHero.Remove(id);

        _globals.ScbaSuit.Remove(id);
        _globals.GodState.Remove(id);
        _globals.InfiniteAmmoState.Remove(id);

        _globals.g_ZombieIdleStates.Remove(id);
        _globals.g_ZombieRegenStates.Remove(id);
        _globals.StopZombieTimers.Remove(id);
        _globals.g_IsInvisible.Remove(id);
        _globals.ThrowerIsZombie.Remove(id);

        _globals.InSwing[id] = false;

        _core.Scheduler.DelayBySeconds(1.0f, () =>
        {
            var playerCount = _helpers.ServerPlayerCount();
            if (playerCount <= 0 && !_globals.ServerIsEmpty)
            {
                _globals.ServerIsEmpty = true;
                _helpers.restartgame();
            }
        });

        var player = _core.PlayerManager.GetPlayer(id);
        if (player != null && player.IsValid)
        {
            _helpers.RemoveGlow(player);
        }
    }

    private void Event_OnTickSpeed()
    {
        var allplayer = _core.PlayerManager.GetAlive();
        foreach (var player in allplayer)
        {
            if (player == null || !player.IsValid)
                continue;

            var pawn = player.PlayerPawn;
            if (pawn == null || !pawn.IsValid)
                continue;

            var Id = player.PlayerID;
            _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
            _globals.IsSurvivor.TryGetValue(Id, out bool IsSurvivor);
            _globals.IsSniper.TryGetValue(Id, out bool IsSniper);
            _globals.IsHero.TryGetValue(Id, out bool IsHero);
            if (IsZombie)
            {
                var zombieConfig = _zombieClassCFG.CurrentValue;
                var specialConfig = _SpecialClassCFG.CurrentValue;
                var zombie = _zombieState.GetZombieClass(Id, zombieConfig.ZombieClassList, specialConfig.SpecialClassList);
                if (zombie == null)
                    continue;

                float zSpeed = zombie.Stats.Speed > 0 ? zombie.Stats.Speed : 1.0f;
                pawn.VelocityModifier = zSpeed;
                pawn.VelocityModifierUpdated();

                float zGravity = zombie.Stats.Gravity;
                pawn.ActualGravityScale = zGravity;
            }
            else if (IsSurvivor)
            {
                float Speed = _mainCFG.CurrentValue.Survivor.SurvivorSpeed > 0 ? _mainCFG.CurrentValue.Survivor.SurvivorSpeed : 3.0f;
                pawn.VelocityModifier = Speed;
                pawn.VelocityModifierUpdated();

                float Gravity = _mainCFG.CurrentValue.Survivor.SurvivorGravity;
                pawn.ActualGravityScale = Gravity;
            }
            else if (IsSniper)
            {
                float Speed = _mainCFG.CurrentValue.Sniper.SniperSpeed > 0 ? _mainCFG.CurrentValue.Sniper.SniperSpeed : 2.0f;
                pawn.VelocityModifier = Speed;
                pawn.VelocityModifierUpdated();

                float Gravity = _mainCFG.CurrentValue.Sniper.SniperGravity;
                pawn.ActualGravityScale = Gravity;
            }
            else if (IsHero)
            {
                float Speed = _mainCFG.CurrentValue.Hero.HeroSpeed > 0 ? _mainCFG.CurrentValue.Hero.HeroSpeed : 2.0f;
                pawn.VelocityModifier = Speed;
                pawn.VelocityModifierUpdated();

                float Gravity = _mainCFG.CurrentValue.Hero.HeroGravity;
                pawn.ActualGravityScale = Gravity;
            }
            else
            {
                float Speed = _mainCFG.CurrentValue.HumanInitialSpeed > 0 ? _mainCFG.CurrentValue.HumanInitialSpeed : 1.0f;
                pawn.VelocityModifier = Speed;
                pawn.VelocityModifierUpdated();

                float Gravity = _mainCFG.CurrentValue.HumanInitialGravity;
                pawn.ActualGravityScale = Gravity;
            }
        }

    }

    private void Event_OnTickNoRecoil()
    {
        var CFG = _mainCFG.CurrentValue;
        if (!CFG.EnableWeaponNoRecoil)
            return;

        foreach (var player in _core.PlayerManager.GetAllPlayers())
        {
            if (player == null || !player.IsValid)
                continue;

            var pawn = player.PlayerPawn;
            if (pawn == null || !pawn.IsValid)
                continue;

            var controller = player.Controller;
            if (controller == null || !controller.IsValid)
                continue;

            var ControllerValue = controller.PlayerPawn.Value;
            if (ControllerValue == null || !ControllerValue.IsValid)
                continue;

            var WeaponServices = ControllerValue.WeaponServices;
            if (WeaponServices == null || !WeaponServices.IsValid)
                continue;

            var weapon = WeaponServices.ActiveWeapon.Value;
            if (weapon == null || !weapon.IsValid)
                continue;

            pawn.AimPunchAngle.Pitch = 0;
            pawn.AimPunchAngle.Yaw = 0;
            pawn.AimPunchAngle.Roll = 0;
            pawn.AimPunchAngleVel.Pitch = 0;
            pawn.AimPunchAngleVel.Yaw = 0;
            pawn.AimPunchAngleVel.Roll = 0;
            pawn.AimPunchTickFraction = 0;
        }
    }

    private void Event_OnHumanTakeDamage(SwiftlyS2.Shared.Events.IOnEntityTakeDamageEvent @event)
    {
        var victim = @event.Entity;
        if (victim == null || !victim.IsValid)
            return;

        var victimPawn = victim.As<CCSPlayerPawn>();
        if (victimPawn == null || !victimPawn.IsValid)
            return;

        var victimController = victimPawn.Controller.Value?.As<CCSPlayerController>();
        if (victimController == null || !victimController.IsValid)
            return;

        var victimPlayer = _core.PlayerManager.GetPlayer((int)(victimController.Index - 1));
        if (victimPlayer == null || !victimPlayer.IsValid)
            return;

        var attacker = @event.Info.Attacker.Value;
        if (attacker == null || !attacker.IsValid)
            return;

        var AttackerPawn = attacker.As<CCSPlayerPawn>();
        if (AttackerPawn == null || !AttackerPawn.IsValid)
            return;

        var AttackerController = AttackerPawn.Controller.Value?.As<CCSPlayerController>();
        if (AttackerController == null || !AttackerController.IsValid)
            return;

        var AttackerPlayer = _core.PlayerManager.GetPlayer((int)(AttackerController.Index - 1));
        if (AttackerPlayer == null || !AttackerPlayer.IsValid)
            return;

        var activeWeapon = AttackerPawn.WeaponServices?.ActiveWeapon.Value;
        if (activeWeapon == null || !activeWeapon.IsValid)
            return;

        var attackerId = AttackerPlayer.PlayerID;
        var victimId = victimPlayer.PlayerID;

        var CFG = _mainCFG.CurrentValue;

        _globals.IsZombie.TryGetValue(attackerId, out bool attackerIsZombie);
        _globals.IsZombie.TryGetValue(victimId, out bool victimIsZombie);
        if (attackerIsZombie || !victimIsZombie)
            return;

        _globals.IsSurvivor.TryGetValue(attackerId, out bool attackerIsSurvivor);
        _globals.IsSniper.TryGetValue(attackerId, out bool attackerIsSniper);
        _globals.IsHero.TryGetValue(attackerId, out bool attackerIsHero);

        if (attackerIsSurvivor || attackerIsSniper || attackerIsHero)
        {
            var config = _mainCFG.CurrentValue;
            if (attackerIsSurvivor && activeWeapon.DesignerName == config.Survivor.SurvivorWeapon)
            {
                @event.Info.Damage *= config.Survivor.SurvivorDamage;
            }
            else if (attackerIsSniper && activeWeapon.DesignerName == config.Sniper.SniperWeapon)
            {
                @event.Info.Damage *= config.Sniper.SniperDamage;
            }
            else if (attackerIsHero)
            {
                @event.Info.Damage *= config.Hero.HeroDamage;
            }

        }

        var AmmoType = @event.Info.AmmoType;
        if(AmmoType == -1)
            return;

        float stunTime = CFG.StunZombieTime;
        _helpers.SetZombieFreezeOrStun(victimPlayer, stunTime);

        bool isheadshot = @event.Info.ActualHitGroup == HitGroup_t.HITGROUP_HEAD;

        //_logger.LogInformation($"Damage Info - Attacker: {AttackerPlayer.Name}, Victim: {victimPlayer.Name}, AmmoType: {@event.Info.AmmoType}, IsHeadshot: {isheadshot}");

        var inflictor = @event.Info.Inflictor.Value;
        if(inflictor == null || !inflictor.IsValid || !inflictor.IsValidEntity)
            return;

        string inflictorname = inflictor.DesignerName;

        float force = CFG.KnockZombieForce;

        _globals.GodState.TryGetValue(victimId, out bool IsGodState);
        if (!IsGodState)
        {
            _helpers.KnockBackZombie(AttackerPlayer, victimPlayer, inflictorname, force, isheadshot, CFG);
        }
    }

    private HookResult OnHumanWeaponFire(EventWeaponFire @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        var pawn = @event.UserIdPawn;
        if (pawn == null || !pawn.IsValid)
            return HookResult.Continue;

        var Id = player.PlayerID;
        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if(IsZombie)
            return HookResult.Continue;

        _globals.IsSurvivor.TryGetValue(Id, out bool IsSurvivor);
        _globals.IsSniper.TryGetValue(Id, out bool IsSniper);
        _globals.IsHero.TryGetValue(Id, out bool IsHero);
        _globals.InfiniteAmmoState.TryGetValue(Id, out bool IsInfiniteAmmoState);


        var CFG = _mainCFG.CurrentValue;

        var activeWeapon = pawn.WeaponServices?.ActiveWeapon.Value;
        if (activeWeapon == null || !activeWeapon.IsValid)
            return HookResult.Continue;

        if(_helpers.CheckIsGrenade(activeWeapon))
            return HookResult.Continue;

        if (CFG.EnableInfiniteReserveAmmo && activeWeapon.ReserveAmmo[0] < 1000)
        {
            activeWeapon.ReserveAmmo[0] = 1000;
        }

        if (_globals.GameInfiniteClipMode)
        {
            activeWeapon.Clip1 = 100;
            activeWeapon.Clip1Updated();
        }
        else if (IsInfiniteAmmoState)
        {
            activeWeapon.Clip1 = 100;
            activeWeapon.Clip1Updated();
        }
        else if(IsSurvivor && activeWeapon.DesignerName == CFG.Survivor.SurvivorWeapon)
        {
            activeWeapon.Clip1 = 100;
            activeWeapon.Clip1Updated();
        }
        else if (IsSniper && activeWeapon.DesignerName == CFG.Sniper.SniperWeapon)
        {
            activeWeapon.Clip1 = 100;
            activeWeapon.Clip1Updated();
        }
        else if (IsHero)
        {
            activeWeapon.Clip1 = 100;
            activeWeapon.Clip1Updated();
        }

        return HookResult.Continue;
    }

    private HookResult CheckRoundWinDeath(EventPlayerDeath @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        if(!_globals.GameStart)
            return HookResult.Continue;

        _service.CheckRoundWinConditions();

        return HookResult.Continue;
    }

    private HookResult CheckRoundWinSpawn(EventPlayerSpawn @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        if (!_globals.GameStart)
            return HookResult.Continue;

        _service.CheckRoundWinConditions();

        return HookResult.Continue;
    }

    private HookResult RandomSpawn(EventPlayerSpawn @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return HookResult.Continue;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out var isZombie);

        _service.RandomSpawnPoint(player, !isZombie);

        return HookResult.Continue;
    }

    private HookResult OnGrenadeThrown(EventGrenadeThrown @event)
    {
        if (!_globals.GameStart)
            return HookResult.Continue;

        var Thrower = @event.UserIdPlayer;
        if (Thrower == null || !Thrower.IsValid)
            return HookResult.Continue;

        var ThrowerId = Thrower.PlayerID;

        _globals.IsZombie.TryGetValue(ThrowerId, out bool isZombie);
        _globals.ThrowerIsZombie[ThrowerId] = isZombie;

        return HookResult.Continue;
    }
    private HookResult OnGrenadeDetonate(EventHegrenadeDetonate @event)
    {
        if(!_globals.GameStart)
            return HookResult.Continue;

        var Thrower = @event.UserIdPlayer;
        if(Thrower == null || !Thrower.IsValid)
            return HookResult.Continue;

        var entityId = @event.EntityID;
        var entity = _core.EntitySystem.GetEntityByIndex<CHEGrenadeProjectile>((uint)entityId);
        if (entity == null || !entity.IsValid)
            return HookResult.Continue;

        var ThrowerId = Thrower.PlayerID;

        var CFG = _mainCFG.CurrentValue;

        if (_globals.ThrowerIsZombie.TryGetValue(ThrowerId, out bool throwerIsZombie) && throwerIsZombie)
        {
            _globals.ThrowerIsZombie.Remove(ThrowerId);

            SwiftlyS2.Shared.Natives.Vector position = new SwiftlyS2.Shared.Natives.Vector(@event.X, @event.Y, @event.Z);
            float radius = CFG.TVirusGrenadeRange;
            _helpers.DrawExpandingRing(position, radius, 0, 255, 0, 125);

            var sound = new SwiftlyS2.Shared.Sounds.SoundEvent(CFG.TVirusGrenadeSound, 1.0f, 1.0f);
            sound.SourceEntityIndex = (int)entity.Index;
            sound.Recipients.AddAllPlayers();
            _core.Scheduler.NextTick(() =>
            {
                sound.Emit();
            });

            var allPlayer = _core.PlayerManager.GetAlive();
            foreach (var human in allPlayer)
            {
                if (human == null || !human.IsValid)
                    continue;

                _globals.IsZombie.TryGetValue(human.PlayerID, out bool isZombie);
                if (isZombie)
                    continue;

                _globals.IsHero.TryGetValue(human.PlayerID, out bool isHero);
                _globals.IsSniper.TryGetValue(human.PlayerID, out bool isSniper);
                _globals.IsSurvivor.TryGetValue(human.PlayerID, out bool isSurvivor);
                if (!CFG.TVirusCanInfectHero && (isHero || isSniper || isSurvivor))
                    continue;

                var pawn = human.PlayerPawn;
                if (pawn == null || !pawn.IsValid)
                    continue;
                // 计算玩家和爆炸位置的距离
                var humanPos = pawn.AbsOrigin;
                if (humanPos == null)
                    continue;

                float distance = MathF.Sqrt(
                    MathF.Pow(humanPos.Value.X - position.X, 2) +
                    MathF.Pow(humanPos.Value.Y - position.Y, 2) +
                    MathF.Pow(humanPos.Value.Z - position.Z, 2)
                );

                if (distance <= radius)
                {
                    _service.Infect(Thrower, human, true);
                }
            }
        }
        else
        {
            _globals.ThrowerIsZombie.Remove(ThrowerId);

            if(!CFG.FireGrenade)
                return HookResult.Continue;

            SwiftlyS2.Shared.Natives.Vector position = new SwiftlyS2.Shared.Natives.Vector(@event.X, @event.Y, @event.Z);
            float radius = CFG.FireGrenadeRange;
            _helpers.DrawExpandingRing(position, radius, 255, 0, 0, 125);

            var sound = new SwiftlyS2.Shared.Sounds.SoundEvent(CFG.FireGrenadeSound, 1.0f, 1.0f);
            sound.SourceEntityIndex = (int)entity.Index;
            sound.Recipients.AddAllPlayers();
            _core.Scheduler.NextTick(() =>
            {
                sound.Emit();
            });

            var allPlayer = _core.PlayerManager.GetAlive();
            foreach (var zombie in allPlayer)
            {
                if (zombie == null || !zombie.IsValid)
                    continue;

                _globals.IsZombie.TryGetValue(zombie.PlayerID, out bool isZombie);
                if (!isZombie)
                    continue;

                var pawn = zombie.PlayerPawn;
                if (pawn == null || !pawn.IsValid)
                    continue;
                // 计算玩家和爆炸位置的距离
                var zombiePos = pawn.AbsOrigin;
                if (zombiePos == null)
                    continue;

                float distance = MathF.Sqrt(
                    MathF.Pow(zombiePos.Value.X - position.X, 2) +
                    MathF.Pow(zombiePos.Value.Y - position.Y, 2) +
                    MathF.Pow(zombiePos.Value.Z - position.Z, 2)
                );

                if (distance <= radius)
                {
                    var zombieConfig = _zombieClassCFG.CurrentValue;
                    var specialConfig = _SpecialClassCFG.CurrentValue;
                    var zombieclass = _zombieState.GetZombieClass(zombie.PlayerID, zombieConfig.ZombieClassList, specialConfig.SpecialClassList);
                    if (zombieclass == null)
                        continue;

                    _helpers.StartIgnite(Thrower, zombie, CFG.FireGrenadeDmg, CFG.FireDmg, CFG.FireGrenadeDuration, zombieclass.Sounds.BurnSound, zombieclass.Stats.ZombieSoundVolume);
                }
            }

        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerBlind(EventPlayerBlind @event)
    {
        var player = @event.UserIdPlayer;
        if(player == null || !player.IsValid)
            return HookResult.Continue;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return HookResult.Continue;

        pawn.BlindUntilTime.Value = _core.Engine.GlobalVars.CurrentTime;

        return HookResult.Continue;
    }
    private HookResult OnFlashbangDetonate(EventFlashbangDetonate @event)
    {
        var entityId = @event.EntityID;
        var entity = _core.EntitySystem.GetEntityByIndex<CFlashbangProjectile>((uint)entityId);
        if (entity == null || !entity.IsValid)
            return HookResult.Continue;

        var CFG = _mainCFG.CurrentValue;
        if(!CFG.LightGrenade)
            return HookResult.Continue;

        float Duration = CFG.LightGrenadeDuration;
        float range = CFG.LightGrenadeRange;
        SwiftlyS2.Shared.Natives.Vector position = new SwiftlyS2.Shared.Natives.Vector(@event.X, @event.Y, @event.Z);

        var light = _helpers.CreateLight(position, range, 255, 255, 255, 255, CFG.LightGrenadeSound);
        if (light == null || !light.IsValid)
            return HookResult.Continue;

        var lightIndex = light.Index;
        _globals.activeLights[lightIndex] = light;
        _globals.lightTimers[lightIndex] = _core.Scheduler.DelayBySeconds(Duration, () => 
        {
            _helpers.RemoveLight(lightIndex);
        });

        return HookResult.Continue;
    }

    private HookResult OnSmokegrenadeDetonate(EventSmokegrenadeDetonate @event)
    {
        var entityId = @event.EntityID;
        var entity = _core.EntitySystem.GetEntityByIndex<CSmokeGrenadeProjectile>((uint)entityId);
        if (entity == null || !entity.IsValid)
            return HookResult.Continue;

        var CFG = _mainCFG.CurrentValue;

        if(!CFG.FreezeGrenade)
            return HookResult.Continue;


        SwiftlyS2.Shared.Natives.Vector position = new SwiftlyS2.Shared.Natives.Vector(@event.X, @event.Y, @event.Z);
        float radius = 500f;
        _helpers.DrawExpandingRing(position, radius, 0, 0, 255, 125);
        var sound = new SwiftlyS2.Shared.Sounds.SoundEvent(CFG.FreezeGrenadeSound, 1.0f, 1.0f);
        sound.SourceEntityIndex = (int)entity.Index;
        sound.Recipients.AddAllPlayers();
        _core.Scheduler.NextTick(() =>
        {
            sound.Emit();
        });


        var allPlayer = _core.PlayerManager.GetAlive();
        foreach (var zombie in allPlayer)
        {
            if (zombie == null || !zombie.IsValid)
                continue;

            _globals.IsZombie.TryGetValue(zombie.PlayerID, out bool isZombie);
            if (!isZombie)
                continue;

            var pawn = zombie.PlayerPawn;
            if (pawn == null || !pawn.IsValid)
                continue;
            // 计算玩家和爆炸位置的距离
            var zombiePos = pawn.AbsOrigin;
            if (zombiePos == null)
                continue;

            float distance = MathF.Sqrt(
                MathF.Pow(zombiePos.Value.X - position.X, 2) +
                MathF.Pow(zombiePos.Value.Y - position.Y, 2) +
                MathF.Pow(zombiePos.Value.Z - position.Z, 2)
            );

            if (distance <= radius)
            {
                _helpers.SetZombieFreezeOrStun(zombie, CFG.FreezeGrenadeDuration, "Glass.BulletImpact");
            }
        }

        if (entity != null && entity.IsValid && entity.IsValidEntity)
        {
            entity.AcceptInput("kill", 0);
        }
        return HookResult.Continue;

    }

    private HookResult OnDecoyFiring(EventDecoyFiring @event)
    {
        var entityId = @event.EntityID;
        var entity = _core.EntitySystem.GetEntityByIndex<CDecoyProjectile>((uint)entityId);
        if (entity == null || !entity.IsValid)
            return HookResult.Continue;

        var player = @event.UserIdPlayer;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        var CFG = _mainCFG.CurrentValue;

        if (!CFG.TelportGrenade)
            return HookResult.Continue;

        SwiftlyS2.Shared.Natives.Vector position = new SwiftlyS2.Shared.Natives.Vector(@event.X, @event.Y, @event.Z);

        var id = player.PlayerID;
        _globals.IsZombie.TryGetValue(id, out bool isZombie);
        if (!isZombie)
        {
            player.Teleport(position);
        }
        if (entity != null && entity.IsValid && entity.IsValidEntity)
        {
            entity.AcceptInput("kill", 0);
        }
        return HookResult.Continue;

    }

}