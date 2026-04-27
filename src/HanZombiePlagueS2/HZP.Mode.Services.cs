using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using static HanZombiePlagueS2.HZPZombieClassCFG;

namespace HanZombiePlagueS2;

public partial class HZPServices
{

    private IPlayer? PickRandomPlayer(List<IPlayer> candidates)
    {
        if (candidates.Count == 0) 
            return null;

        return candidates[Random.Shared.Next(candidates.Count)];
    }

    public void InfectMotherPlayer(IPlayer player, bool isMother = false)
    {
        if (player == null || !player.IsValid) 
            return;

        var zombieConfig = _zombieClassCFG.CurrentValue;
        var zombieClasses = zombieConfig.ZombieClassList;

        // 根据玩家偏好选择类
        var preference = _zombieState.GetPlayerPreference(player.PlayerID, player.SteamID);
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
            PlayerSelectSoundtoAll(selectedClass.Sounds.SoundInfect, selectedClass.Stats.ZombieSoundVolume);
            posszombie(player, selectedClass, isMother);

            if (_api != null)
                _api.NotifyMotherZombieSelected(player);

        }
    }
    public void SwitchMode()
    {
        var mode = _gameMode.CurrentMode;
        switch (mode)
        {
            case GameModeType.Normal:
            case GameModeType.NormalInfection:
                NormalInfectionMode();
                break;
            case GameModeType.MultiInfection:
                MultiInfectionMode();
                break;
            case GameModeType.Nemesis:
                NemesisMode();
                break;
            case GameModeType.Survivor:
                SurvivorMode();
                break;
            case GameModeType.Swarm:
                SwarmMode();
                break;
            case GameModeType.Plague:
                PlagueMode();
                break;
            case GameModeType.Assassin:
                AssassinMode();
                break;
            case GameModeType.Sniper:
                SniperMode();
                break;
            case GameModeType.AVS:
                AVSMode();
                break;
            case GameModeType.Hero:
                HeroMode();
                break;
            default:
                NormalInfectionMode();
                break;
        }

    }

    public void HeroMode()
    {
        var config = _mainCFG.CurrentValue.NormalInfection;
        SelectMotherZombie(config.MotherZombieCount);
        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());

    }

    public void NormalInfectionMode()
    {
        var config = _mainCFG.CurrentValue.NormalInfection;
        SelectMotherZombie(config.MotherZombieCount);
        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }

    public void MultiInfectionMode()
    {
        var config = _mainCFG.CurrentValue.MultiInfection;
        SelectMotherZombie(config.MotherZombieCount);
        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }

    public void NemesisMode()
    {
        var allplayers = GetValidPlayers();
        if (allplayers.Count == 0) 
            return;

        var target = PickRandomPlayer(allplayers);
        if(target == null || !target.IsValid)
            return;

        SetupNemesis(target);
        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }

    public void SurvivorMode()
    {
        var allplayers = GetValidPlayers();
        if (allplayers.Count == 0) return;

        // 选择幸存者
        var survivor = PickRandomPlayer(allplayers);
        if (survivor == null || !survivor.IsValid)
            return;

        allplayers.Remove(survivor);

        // 其他人全部变成丧尸
        foreach (var p in allplayers)
        {
            InfectMotherPlayer(p, false);
        }

        // 设置幸存者属性
        SetupSurvivor(survivor);

        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }

    public void SwarmMode()
    {
        var allplayers = GetValidPlayers();
        Random.Shared.Shuffle(CollectionsMarshal.AsSpan(allplayers));

        int zombieCount = allplayers.Count / 2;
        for (int i = 0; i < zombieCount; i++)
        {
            InfectMotherPlayer(allplayers[i], true);
        }

        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }
    public void PlagueMode()
    {
        var allplayers = GetValidPlayers();
        if (allplayers.Count == 0)
            return;

        Random.Shared.Shuffle(CollectionsMarshal.AsSpan(allplayers));

        // 50% 变成丧尸
        int zombieCount = allplayers.Count / 2;
        for (int i = 0; i < zombieCount; i++)
        {
            InfectMotherPlayer(allplayers[i], true);
        }

        // 选择幸存者（确保索引存在）
        if (allplayers.Count > zombieCount)
        {
            var survivor = allplayers[Math.Min(zombieCount, allplayers.Count - 1)];
            SetupSurvivor(survivor);
        }

        // 选择复仇女神（从丧尸中选一个）
        if (zombieCount > 0)
        {
            var nemesis = allplayers[0];
            SetupNemesis(nemesis);
        }

        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }

    public void AVSMode()
    {
        var allplayers = GetValidPlayers();
        if (allplayers.Count == 0)
            return;

        Random.Shared.Shuffle(CollectionsMarshal.AsSpan(allplayers));

        // 50% 变成丧尸
        int zombieCount = allplayers.Count / 2;
        for (int i = 0; i < zombieCount; i++)
        {
            InfectMotherPlayer(allplayers[i], true);
        }

        if (allplayers.Count > zombieCount)
        {
            var Sniper = allplayers[Math.Min(zombieCount, allplayers.Count - 1)];
            SetupSniper(Sniper);
        }

        if (zombieCount > 0)
        {
            var Assassin = allplayers[0];
            SetupAssassin(Assassin);
        }

        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }

    public void AssassinMode()
    {
        var allplayers = GetValidPlayers();
        if (allplayers.Count == 0) 
            return;

        var target = PickRandomPlayer(allplayers);
        if (target == null || !target.IsValid) 
            return;

        SetupAssassin(target);
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
        
    }

    public void SniperMode()
    {
        var allplayers = GetValidPlayers();
        var sniper = PickRandomPlayer(allplayers);

        if (sniper == null || !sniper.IsValid)
            return;

        allplayers.Remove(sniper);

        // 其他人变成丧尸
        foreach (var p in allplayers)
        {
            InfectMotherPlayer(p,  false);
        }

        // 设置狙击手属性
        SetupSniper(sniper);

        
        _helpers.SendCenterToAllT(_gameMode.GetTramslationsModeName());
    }

    public void SetupHero()
    {
        var mode = _gameMode.CurrentMode;
        if (mode != GameModeType.Hero)
            return;

        if(_globals.IsheroSetup)
            return;

        var allPlayers = _core.PlayerManager.GetAlive();
        var aliveHumans = allPlayers.Where(p =>
        {
            var id = p.PlayerID;
            return _globals.IsZombie.TryGetValue(id, out var isZombie) && !isZombie;
        }).ToList();

        if (aliveHumans.Count == 0)
            return;

        int heroCount = _mainCFG.CurrentValue.Hero.HeroCount;

        //_logger.LogInformation($"[调试] 人类数量: {aliveHumans.Count}, 英雄配置: {heroCount}");

        // 只有当人类数量 <= 英雄数量时，才有人成为英雄
        if (aliveHumans.Count > heroCount)
        {
            //移除所有英雄状态
            //_logger.LogInformation($"[调试] 移除所有英雄状态");
            foreach (var player in aliveHumans)
            {
                _globals.IsHero.Remove(player.PlayerID);
            }
            return;
        }

        // 人类数量 <= 英雄数量，所有人都成为英雄
        //_logger.LogInformation($"[调试] 所有 {aliveHumans.Count} 个人类成为英雄");
        foreach (var player in aliveHumans)
        {
            posshero(player);
        }

        _globals.IsheroSetup = true;

    }

    public void SetupSurvivor(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsSurvivor[Id] = true;

        posshuman(player, true);

        if (_api != null)
            _api.NotifySurvivorSelected(player);
    }

    public void SetupSniper(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsSniper[Id] = true;

        posshuman(player, false);

        if (_api != null)
            _api.NotifySniperSelected(player);
    }

    public void SetupMotherZombie(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;
        _globals.IsMother[Id] = true;

        var specialConfig = _specialClassCFG.CurrentValue;
        var MotherZombieClass = specialConfig.SpecialClassList.FirstOrDefault(c =>
            c.Name == _mainCFG.CurrentValue.NormalInfection.MotherZombieNames);

        if (MotherZombieClass != null)
        {
            var zombieClass = new ZombieClass
            {
                Name = MotherZombieClass.Name,
                Stats = new ZombieStats
                {
                    MotherZombieHealth = MotherZombieClass.Stats.MotherZombieHealth,
                    Health = MotherZombieClass.Stats.Health,
                    Speed = MotherZombieClass.Stats.Speed,
                    Damage = MotherZombieClass.Stats.Damage,
                    Gravity = MotherZombieClass.Stats.Gravity,
                    Fov = MotherZombieClass.Stats.Fov,
                    EnableRegen = MotherZombieClass.Stats.EnableRegen,
                    HpRegenSec = MotherZombieClass.Stats.HpRegenSec,
                    HpRegenHp = MotherZombieClass.Stats.HpRegenHp,
                    ZombieSoundVolume = MotherZombieClass.Stats.ZombieSoundVolume,
                    IdleInterval = MotherZombieClass.Stats.IdleInterval,
                },
                Models = new ZombieModels
                {
                    ModelPath = MotherZombieClass.Models.ModelPath,
                    CustomKinfeModelPath = MotherZombieClass.Models.CustomKinfeModelPath
                },
                Sounds = new ZombieSounds
                {
                    SoundInfect = MotherZombieClass.Sounds.SoundInfect,
                    SoundPain = MotherZombieClass.Sounds.SoundPain,
                    SoundHurt = MotherZombieClass.Sounds.SoundHurt,
                    SoundDeath = MotherZombieClass.Sounds.SoundDeath,
                    IdleSound = MotherZombieClass.Sounds.IdleSound,
                    RegenSound = MotherZombieClass.Sounds.RegenSound,
                    BurnSound = MotherZombieClass.Sounds.BurnSound,
                    ExplodeSound = MotherZombieClass.Sounds.SoundInfect,
                    HitSound = MotherZombieClass.Sounds.HitSound,
                    HitWallSound = MotherZombieClass.Sounds.HitWallSound,
                    SwingSound = MotherZombieClass.Sounds.SwingSound
                }
            };
            posszombie(player, zombieClass, true);

            if (_api != null)
                _api.NotifyMotherZombieSelected(player);
        }
    }

    public void SetupNemesis(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;
        _globals.IsNemesis[Id] = true;

        var specialConfig = _specialClassCFG.CurrentValue;
        var nemesisClass = specialConfig.SpecialClassList.FirstOrDefault(c =>
            c.Name == _mainCFG.CurrentValue.Nemesis.NemesisNames);

        if (nemesisClass != null)
        {
            var zombieClass = new ZombieClass
            {
                Name = nemesisClass.Name,
                Stats = new ZombieStats
                {
                    MotherZombieHealth = nemesisClass.Stats.MotherZombieHealth,
                    Health = nemesisClass.Stats.Health,
                    Speed = nemesisClass.Stats.Speed,
                    Damage = nemesisClass.Stats.Damage,
                    Gravity = nemesisClass.Stats.Gravity,
                    Fov = nemesisClass.Stats.Fov,
                    EnableRegen = nemesisClass.Stats.EnableRegen,
                    HpRegenSec = nemesisClass.Stats.HpRegenSec,
                    HpRegenHp = nemesisClass.Stats.HpRegenHp,
                    ZombieSoundVolume = nemesisClass.Stats.ZombieSoundVolume,
                    IdleInterval = nemesisClass.Stats.IdleInterval,
                },
                Models = new ZombieModels
                {
                    ModelPath = nemesisClass.Models.ModelPath,
                    CustomKinfeModelPath = nemesisClass.Models.CustomKinfeModelPath
                },
                Sounds = new ZombieSounds
                {
                    SoundInfect = nemesisClass.Sounds.SoundInfect,
                    SoundPain = nemesisClass.Sounds.SoundPain,
                    SoundHurt = nemesisClass.Sounds.SoundHurt,
                    SoundDeath = nemesisClass.Sounds.SoundDeath,
                    IdleSound = nemesisClass.Sounds.IdleSound,
                    RegenSound = nemesisClass.Sounds.RegenSound,
                    BurnSound = nemesisClass.Sounds.BurnSound,
                    ExplodeSound = nemesisClass.Sounds.SoundInfect,
                    HitSound = nemesisClass.Sounds.HitSound,
                    HitWallSound = nemesisClass.Sounds.HitWallSound,
                    SwingSound = nemesisClass.Sounds.SwingSound
                }
            };
            posszombie(player, zombieClass, true);

            var sessionId = player.SessionId;
            var roundGeneration = _helpers.GetCurrentRoundGeneration();
            _helpers.RunNextWorldUpdateForPlayer(Id, sessionId, roundGeneration, (currentPlayer, _) => 
            {
                _helpers.SetGlow(currentPlayer, 255, 0, 0, 255);
            }, requireAlive: true);
            

            _helpers.SendChatToAllT("GameInfoBecomeNemesis", player.Name);

            if (_api != null)
                _api.NotifyNemesisSelected(player);

        }
    }

        
    public void SetupAssassin(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;
        _globals.IsAssassin[Id] = true;

        var specialConfig = _specialClassCFG.CurrentValue;
        var AssassinClass = specialConfig.SpecialClassList.FirstOrDefault(c =>
            c.Name == _mainCFG.CurrentValue.Assassin.AssassinNames);

        if (AssassinClass != null)
        {
            var zombieClass = new ZombieClass
            {
                Name = AssassinClass.Name,
                Stats = new ZombieStats
                {
                    MotherZombieHealth = AssassinClass.Stats.MotherZombieHealth,
                    Health = AssassinClass.Stats.Health,
                    Speed = AssassinClass.Stats.Speed,
                    Damage = AssassinClass.Stats.Damage,
                    Gravity = AssassinClass.Stats.Gravity,
                    Fov = AssassinClass.Stats.Fov,
                    EnableRegen = AssassinClass.Stats.EnableRegen,
                    HpRegenSec = AssassinClass.Stats.HpRegenSec,
                    HpRegenHp = AssassinClass.Stats.HpRegenHp,
                    ZombieSoundVolume = AssassinClass.Stats.ZombieSoundVolume,
                    IdleInterval = AssassinClass.Stats.IdleInterval,
                },
                Models = new ZombieModels
                {
                    ModelPath = AssassinClass.Models.ModelPath,
                    CustomKinfeModelPath = AssassinClass.Models.CustomKinfeModelPath
                },
                Sounds = new ZombieSounds
                {
                    SoundInfect = AssassinClass.Sounds.SoundInfect,
                    SoundPain = AssassinClass.Sounds.SoundPain,
                    SoundHurt = AssassinClass.Sounds.SoundHurt,
                    SoundDeath = AssassinClass.Sounds.SoundDeath,
                    IdleSound = AssassinClass.Sounds.IdleSound,
                    RegenSound = AssassinClass.Sounds.RegenSound,
                    BurnSound = AssassinClass.Sounds.BurnSound,
                    ExplodeSound = AssassinClass.Sounds.SoundInfect,
                    HitSound = AssassinClass.Sounds.HitSound,
                    HitWallSound = AssassinClass.Sounds.HitWallSound,
                    SwingSound = AssassinClass.Sounds.SwingSound
                }
            };
            posszombie(player, zombieClass, true);

            _helpers.SendChatToAllT("GameInfoBecomeAssassin", player.Name);

            if (_api != null)
                _api.NotifyAssassinSelected(player);
        }


    }

    public void StartAssassinInvisibilityTimer(float configDist, int expectedRoundGeneration)
    {
        _globals.AssassinTimer?.Cancel();
        _globals.AssassinTimer = null;

        CancellationTokenSource? assassinTimer = null;
        assassinTimer = _core.Scheduler.RepeatBySeconds(0.2f, () =>
        {
            if (!_helpers.IsRoundGenerationCurrent(expectedRoundGeneration))
            {
                assassinTimer?.Cancel();
                return;
            }

            AssassinInvisibilityTick(configDist);
        });
        _globals.AssassinTimer = assassinTimer;

        _core.Scheduler.StopOnMapChange(_globals.AssassinTimer);
    }


    public void AssassinInvisibilityTick(float configDist)
    {
        float distSqrThreshold = configDist * configDist;

        var players = _core.PlayerManager.GetAlive();

        foreach (var assassin in players)
        {
            if (assassin == null || !assassin.IsValid)
                continue;

            var id = assassin.PlayerID;

            // 不是暗杀者 → 跳过
            _globals.IsAssassin.TryGetValue(id, out bool isAssassin);
            if (!isAssassin)
            {
                // 如果之前是隐身，强制恢复
                if (_globals.g_IsInvisible.TryGetValue(id, out bool wasInvisible) && wasInvisible)
                {
                    _helpers.SetUnInvisibility(assassin);
                    _globals.g_IsInvisible[id] = false;
                }
                continue;
            }

            var pawn = assassin.PlayerPawn;
            if (pawn == null || !pawn.IsValid)
                continue;

            // 死亡 → 强制可见
            if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            {
                if (_globals.g_IsInvisible.TryGetValue(id, out bool wasInvisible) && wasInvisible)
                {
                    pawn.Render.A = 255;
                    pawn.RenderUpdated();
                    _globals.g_IsInvisible[id] = false;
                }
                continue;
            }

            var assassinOrigin = pawn.AbsOrigin;
            if (assassinOrigin == null)
                continue;

            bool hasEnemyInRange = false;

            foreach (var enemy in players)
            {
                if (enemy == null || enemy == assassin || !enemy.IsValid)
                    continue;

                _globals.IsZombie.TryGetValue(enemy.PlayerID, out bool isZombie);
                if (isZombie)
                    continue;

                var enemyPawn = enemy.PlayerPawn;
                if (enemyPawn?.IsValid != true)
                    continue;

                var enemyOrigin = enemyPawn.AbsOrigin;
                if (enemyOrigin == null)
                    continue;

                float distSqr = _helpers.DistanceSquared(
                    assassinOrigin.Value,
                    enemyOrigin.Value
                );

                if (distSqr <= distSqrThreshold)
                {
                    hasEnemyInRange = true;
                    break;
                }
            }

            bool shouldBeInvisible = !hasEnemyInRange;

            if (!_globals.g_IsInvisible.TryGetValue(id, out bool current))
                current = false;

            if (current != shouldBeInvisible)
            {
                if (shouldBeInvisible)
                    _helpers.SetInvisibility(assassin);
                else
                    _helpers.SetUnInvisibility(assassin);

                _globals.g_IsInvisible[id] = shouldBeInvisible;
            }
        }
    }

    public void StopAssassinTimer()
    {
        if (_globals.AssassinTimer == null)
            return;

        _globals.AssassinTimer.Cancel();
        _globals.AssassinTimer = null;

        CleanupAssassinInvisibility();
    }

    private void CleanupAssassinInvisibility()
    {
        foreach (var player in _core.PlayerManager.GetAllPlayers())
        {
            if (player == null || !player.IsValid)
                continue;

            var pawn = player.PlayerPawn;
            if (pawn == null || !pawn.IsValid)
                continue;

            // 强制恢复可见
            if (pawn.Render.A != 255)
            {
                pawn.Render.A = 255;
                pawn.RenderUpdated();
            }
        }

        // 清状态缓存
        _globals.g_IsInvisible.Clear();
    }
    
    private void posshuman(IPlayer player, bool isSurvivor = false)
    {
        if (player == null || !player.IsValid) 
            return;

        var Id = player.PlayerID;
        var sessionId = player.SessionId;
        var roundGeneration = _helpers.GetCurrentRoundGeneration();

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid) 
            return;

        var ws = pawn.WeaponServices;
        if (ws == null || !ws.IsValid)
            return;

        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var config = _mainCFG.CurrentValue;
        string classname = isSurvivor ? config.Survivor.SurvivorWeapon : config.Sniper.SniperWeapon;
        string customname = isSurvivor ? config.Survivor.CustomWeaponName : config.Sniper.CustomWeaponName;
        ws.DropWeaponBySlot(gear_slot_t.GEAR_SLOT_RIFLE);
        var weapon = Is.GiveItem<CCSWeaponBase>(classname);
        ushort DefinitionIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;
        weapon.AcceptInput("ChangeSubclass", classname);
        weapon.AttributeManager.Item.Initialized = true;
        weapon.AttributeManager.Item.ItemDefinitionIndex = DefinitionIndex;
        weapon.AttributeManager.Item.CustomName = customname;
        weapon.AttributeManager.Item.CustomNameOverride = customname;
        weapon.AttributeManager.Item.CustomNameUpdated();


        string model = isSurvivor ? config.Survivor.ModelsPath: config.Sniper.ModelsPath;

        _helpers.RunNextWorldUpdateForPlayer(Id, sessionId, roundGeneration, (currentPlayer, currentPawn) =>
        {
            _helpers.SetPlayerModelFixed(currentPawn, model);
            _helpers.SetGlow(currentPlayer, 0, 0, 255, 255);
        }, requireAlive: true);

        int Health = isSurvivor ? config.Survivor.SurvivorHealth : config.Sniper.SniperHealth;
        pawn.MaxHealth = Health;
        pawn.MaxHealthUpdated();
        pawn.Health = Health;
        pawn.HealthUpdated();

        float Speed = isSurvivor ? config.Survivor.SurvivorSpeed : config.Sniper.SniperSpeed;
        pawn.VelocityModifier = Speed;
        pawn.VelocityModifierUpdated();

        
        string info = isSurvivor ? "GameInfoBecomeSurvivor" : "GameInfoBecomeSniper";
        _helpers.SendChatToAllT(info, player.Name);

    }

    public void posshero(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;
        var sessionId = player.SessionId;
        var roundGeneration = _helpers.GetCurrentRoundGeneration();

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        _globals.IsHero[Id] = true;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        player.SendMessage(MessageType.Center, _helpers.T(player, "GameModeBecomeHero"));
        _helpers.SendChatToAllT("GameInfoBecomeHero", player.Name);
        var ws = pawn.WeaponServices;
        if (ws == null || !ws.IsValid)
            return;



        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var config = _mainCFG.CurrentValue;


        string model = config.Hero.ModelsPath;

        _helpers.RunNextWorldUpdateForPlayer(Id, sessionId, roundGeneration, (currentPlayer, currentPawn) =>
        {
            _helpers.SetPlayerModelFixed(currentPawn, model);
            _helpers.SetGlow(currentPlayer, 255, 230, 140, 255);
        }, requireAlive: true);

        int Health = config.Hero.HeroHealth;
        pawn.MaxHealth = Health;
        pawn.MaxHealthUpdated();
        pawn.Health = Health;
        pawn.HealthUpdated();

        float Speed = config.Hero.HeroSpeed;
        pawn.VelocityModifier = Speed;
        pawn.VelocityModifierUpdated();


        if (_api != null)
            _api.NotifyHeroSelected(player);

    }


    private List<IPlayer> GetValidPlayers()
    {
        var allplayer = _core.PlayerManager.GetAllPlayers();
        var result = new List<IPlayer>();

        foreach (var p in allplayer)
        {
            if (p == null || !p.IsValid) continue;
            var id = p.PlayerID;
            bool isZombie = false;
            _globals.IsZombie.TryGetValue(id, out isZombie);
            if (!isZombie)
                result.Add(p);
        }
        return result;
    }

}
