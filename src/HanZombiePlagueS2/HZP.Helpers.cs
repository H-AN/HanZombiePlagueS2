
using System.Diagnostics;
using System.Security.AccessControl;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Cecil.Cil;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Convars;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Helpers;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Sounds;
using static Dapper.SqlMapper;
using static HanZombiePlagueS2.HanZombiePlagueS2;
using static HanZombiePlagueS2.HZPVoxCFG;
using static Mono.CompilerServices.SymbolWriter.CodeBlockEntry;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace HanZombiePlagueS2;

public partial class HZPHelpers
{
    private readonly ILogger<HZPHelpers> _logger;
    private readonly ISwiftlyCore _core;
    private readonly HZPGlobals _globals;

    public HZPHelpers(ISwiftlyCore core, ILogger<HZPHelpers> logger,
        HZPGlobals globals)
    {
        _core = core;
        _logger = logger;
        _globals = globals;
    }

    public int? ServerPlayerCount()
    {
        var allplayer = _core.PlayerManager.GetAllPlayers();
        var list = new List<IPlayer>();
        foreach (var player in allplayer)
        {
            if (player == null || !player.IsValid)
                continue;

            list.Add(player);
        }
        return list.Count;
    }

    public void DropAllWeapon(IPlayer p)
    {
        var pawn = p.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        var ws = pawn.WeaponServices;
        if (ws == null || !ws.IsValid)
            return;

        ws.DropWeaponBySlot(gear_slot_t.GEAR_SLOT_RIFLE);
        ws.DropWeaponBySlot(gear_slot_t.GEAR_SLOT_PISTOL);
        ws.DropWeaponBySlot(gear_slot_t.GEAR_SLOT_GRENADES);
    }

    public void TerminateRound(RoundEndReason reason, float delay)
    {
        var gameRules = _core.EntitySystem.GetGameRules();
        if (gameRules is not { IsValid: true, WarmupPeriod: false })
            return;

        gameRules.TerminateRound(reason, delay);
    }


    public void SetTeamScore(Team team) //设置队伍分数
    {
        var teamManagers = _core.EntitySystem.GetAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");

        foreach (var teamManager in teamManagers)
        {
            if ((int)team == teamManager.TeamNum)
            {
                teamManager.Score += 1;
                teamManager.ScoreUpdated();
            }
        }
    }

    public string? RandomSelectSound(string sound)
    {
        if (string.IsNullOrWhiteSpace(sound))
            return null;

        var items = sound
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();

        if (items.Length == 0) return null;

        return items.Length == 1 ? items[0] : items[Random.Shared.Next(items.Length)];
    }

    public void SetAmbSounds(HZPMainCFG CFG, HZPGlobals _globals)
    {
        if (!string.IsNullOrWhiteSpace(CFG.AmbSound))
        {
            _globals.g_hAmbMusic?.Cancel();
            _globals.g_hAmbMusic = null;
            _globals.g_hAmbMusic = _core.Scheduler.DelayAndRepeatBySeconds(0.1f, CFG.AmbSoundLoopTime, () => PlayAmbSound(CFG.AmbSound, CFG.AmbSoundVolume));
            _core.Scheduler.StopOnMapChange(_globals.g_hAmbMusic);
        }
    }
    public void PlayAmbSound(string sounds, float Volume) //播放环境音乐
    {
        if (!string.IsNullOrWhiteSpace(sounds))
        {
            var toPlay = RandomSelectSound(sounds);
            if (toPlay != null)
            {
                EmitSoundToAll(toPlay, Volume);
            }
        }
    }

    public void EmitSoundFormPlayer(IPlayer player, string SoundPath, float Volume)
    {
        if (!string.IsNullOrEmpty(SoundPath))
        {
            var pwan = player.PlayerPawn;
            if (pwan == null || !pwan.IsValid)
                return;

            var sound = new SwiftlyS2.Shared.Sounds.SoundEvent(SoundPath, Volume, 1.0f);
            sound.SourceEntityIndex = (int)pwan.Index;
            sound.Recipients.AddAllPlayers();
            _core.Scheduler.NextTick(() =>
            {
                sound.Emit();
            });
        }
    }

    public void EmitSoundToAll(string SoundPath, float Volume)
    {
        if (!string.IsNullOrEmpty(SoundPath))
        {
            var sound = new SwiftlyS2.Shared.Sounds.SoundEvent(SoundPath, Volume, 1.0f);
            sound.SourceEntityIndex = -1;
            sound.Recipients.AddAllPlayers();
            _core.Scheduler.NextTick(() =>
            {
                sound.Emit();
            });
        }
    }

    public RoundVox? PickRandomActiveGroup(IEnumerable<RoundVox> groups)
    {
        if (groups == null)
            return null;

        var enabled = groups.Where(g => g.Enable).ToList();
        if (enabled.Count == 0)
            return null;

        return enabled[Random.Shared.Next(enabled.Count)];
    }

    public bool IsPlayerUsingKnife(CCSPlayerController controller)
    {
        if (controller == null || !controller.IsValid)
            return false;

        var pawn = controller.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid)
            return false;

        var weaponServices = pawn.WeaponServices;
        if (weaponServices == null || !weaponServices.IsValid)
            return false;

        var activeWeapon = weaponServices.ActiveWeapon.Value;
        if (activeWeapon == null || !activeWeapon.IsValid)
            return false;

        return activeWeapon.DesignerName == "weapon_knife";
    }

    public void RemoveHostage()
    {
        var hostage = _core.EntitySystem.GetAllEntitiesByClass<CHostage>().ToList();
        foreach (var entity in hostage)
        {
            if (entity != null && entity.IsValid && entity.IsValidEntity)
            {
                entity.Despawn();
            }
        }
    }

    public void SwitchAllPlayerTeam()
    {
        var allplayer = _core.PlayerManager.GetAllPlayers();
        foreach (var player in allplayer)
        {
            if (player == null || !player.IsValid)
                continue;

            player.SwitchTeam(Team.CT);
        }
    }
    public void restartgame()
    {
        SendChatToAllT("ServerGameBegin");
        _core.Scheduler.DelayBySeconds(1.0f, () =>
        {
            TerminateRound(RoundEndReason.RoundDraw, 5.0f);
        });
    }
    public void SetNoBlock(IPlayer player) //碰撞体积关闭
    {
        if (player == null || !player.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        pawn.Collision.CollisionGroup = (byte)CollisionGroup.Debris;
        pawn.CollisionRulesChanged();
    }

    public void ChangeKnife(IPlayer player, bool isZombie, bool customKnife, string knifepath = "")
    {
        if (player == null || !player.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return;

        if (controller.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var ws = pawn.WeaponServices;
        if (ws == null || !ws.IsValid)
            return;

        ws.DropWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);

        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var weapon = Is.GiveItem<CCSWeaponBase>("weapon_knife");
        if (weapon == null || !weapon.IsValid)
            return;

        if (isZombie)
        {
            if (customKnife)
            {
                weapon.AcceptInput("ChangeSubclass", "42");
                weapon.AttributeManager.Item.Initialized = true;
                weapon.AttributeManager.Item.ItemDefinitionIndex = 42;
                weapon.SetModel(knifepath);
                weapon.AttributeManager.Item.CustomName = T(player, "ZombieClaw");
                weapon.AttributeManager.Item.CustomNameOverride = T(player, "ZombieClaw");
                weapon.AttributeManager.Item.CustomNameUpdated();
            }
            else
            {
                weapon.AcceptInput("ChangeSubclass", "507");
                weapon.AttributeManager.Item.Initialized = true;
                weapon.AttributeManager.Item.ItemDefinitionIndex = 507;
                weapon.SetModel("");
                weapon.AttributeManager.Item.CustomName = T(player, "ZombieClaw");
                weapon.AttributeManager.Item.CustomNameOverride = T(player, "ZombieClaw");
                weapon.AttributeManager.Item.CustomNameUpdated();
            }
        }
        else
        {
            weapon.AcceptInput("ChangeSubclass", "42");
            weapon.AttributeManager.Item.Initialized = true;
            weapon.AttributeManager.Item.ItemDefinitionIndex = 42;
        }

    }

    public void SetInvisibility(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;


        const uint EF_NODRAW = 32;
        const uint EF_NODRAW_BUT_TRANSMIT = 1024;
        pawn.Effects |= (EF_NODRAW | EF_NODRAW_BUT_TRANSMIT);
        pawn.EffectsUpdated();
        pawn.RenderMode = RenderMode_t.kRenderTransAlpha;
        pawn.RenderModeUpdated();
        pawn.Render.A = 0;
        pawn.RenderUpdated();

    }


    public void SetUnInvisibility(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        const uint EF_NODRAW = 32;
        const uint EF_NODRAW_BUT_TRANSMIT = 1024;
        pawn.Effects &= ~(EF_NODRAW | EF_NODRAW_BUT_TRANSMIT);
        pawn.EffectsUpdated();
        pawn.RenderMode = RenderMode_t.kRenderNormal;
        pawn.RenderModeUpdated();
        pawn.Render.A = 255;
        pawn.RenderUpdated();
    }

    public float DistanceSquared(Vector a, Vector b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        float dz = a.Z - b.Z;
        return dx * dx + dy * dy + dz * dz;
    }

    public void SetGlow(IPlayer player, int ColorR, int ColorG, int ColorB, int ColorA)
    {
        if (player == null || !player.IsValid)
            return;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        CBaseModelEntity modelRelay = _core.EntitySystem.CreateEntity<CBaseModelEntity>();
        CBaseModelEntity modelGlow = _core.EntitySystem.CreateEntity<CBaseModelEntity>();
        if (modelRelay == null || !modelRelay.IsValidEntity || !modelRelay.IsValid ||
            modelGlow == null || !modelGlow.IsValidEntity || !modelGlow.IsValid)
            return;

        var modelRelayHandle = _core.EntitySystem.GetRefEHandle(modelRelay);
        var modelGlowHandle = _core.EntitySystem.GetRefEHandle(modelGlow);
        if (!modelRelayHandle.IsValid || !modelGlowHandle.IsValid)
            return;


        string modelName = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
        if (string.IsNullOrEmpty(modelName))
            return;

        _core.Scheduler.NextWorldUpdate(() =>
        {

            modelRelay.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= unchecked((uint)~(1 << 2));
            modelRelay.SetModel(modelName);
            modelRelay.Spawnflags = 256u;
            modelRelay.Render = new Color(1, 1, 1, 1);
            modelRelay.RenderMode = RenderMode_t.kRenderNone;
            modelRelay.DispatchSpawn();

            modelGlow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= unchecked((uint)~(1 << 2));
            modelGlow.SetModel(modelName);
            modelGlow.Spawnflags = 256u;
            modelGlow.DispatchSpawn();
            modelGlow.Glow.GlowColorOverride = new Color(ColorR, ColorG, ColorB, ColorA);
            modelGlow.Glow.GlowRange = 5000;
            modelGlow.Glow.GlowTeam = -1;
            modelGlow.Glow.GlowType = 3;
            modelGlow.Glow.GlowRangeMin = 100;


            modelRelay.AcceptInput("FollowEntity", "!activator", pawn, modelRelay);
            modelGlow.AcceptInput("FollowEntity", "!activator", modelRelay, modelGlow);

        });

        _globals.GlowEntity.Add(controller, new GlowEntity()
        {
            Glow = modelGlow,
            Relay = modelRelay
        });
    }

    public void RemoveGlow(IPlayer player)
    {
        if (player == null || !player.IsValid || !_core.PlayerManager.IsPlayerOnline(player.PlayerID))
            return;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return;
        if (_globals.GlowEntity.TryGetValue(controller, out var glowEntity))
        {
            if (glowEntity.Relay != null && glowEntity.Relay.IsValid)
            {
                var relayHandle = _core.EntitySystem.GetRefEHandle(glowEntity.Relay);
                if (relayHandle.IsValid && relayHandle.Value != null && relayHandle.Value.IsValid)
                {
                    relayHandle.Value.AcceptInput("Kill", 0);
                }
            }

            if (glowEntity.Glow != null && glowEntity.Glow.IsValid)
            {
                var glowHandle = _core.EntitySystem.GetRefEHandle(glowEntity.Glow);
                if (glowHandle.IsValid && glowHandle.Value != null && glowHandle.Value.IsValid)
                {
                    glowHandle.Value.AcceptInput("Kill", 0);
                }
            }

            _globals.GlowEntity.Remove(controller);
        }
    }

    public void SetFov(IPlayer player, int fov)
    {
        if (player == null || !player.IsValid)
            return;

        var Pawn = player.PlayerPawn;
        if (Pawn == null || !Pawn.IsValid)
            return;

        var Controller = player.Controller;
        if (Controller == null || !Controller.IsValid)
            return;

        if (Controller.DesiredFOV != fov)
        {
            Controller.DesiredFOV = (uint)fov;
            Controller.DesiredFOVUpdated();
        }
    }
    public void ShakeZombie(IPlayer zombie)
    {
        if (zombie == null || !zombie.IsValid)
            return;

        var zPawn = zombie.PlayerPawn;
        if (zPawn == null || !zPawn.IsValid)
            return;

        var zController = zombie.Controller;
        if (zController == null || !zController.IsValid)
            return;

        //if(!zController.PawnIsAlive)
        if (zController.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var shake = _core.NetMessage.Create<CUserMessageShake>();
        shake.Amplitude = 30.0f;
        shake.Frequency = 80.0f;
        shake.Duration = 1.5f;

        shake.SendToPlayer(zombie.PlayerID);
    }

    public void BuildSpawnCache()
    {
        _globals.spawnCache.Clear();

        _globals.spawnCache[SpawnType.CT] = Collect("info_player_counterterrorist");
        _globals.spawnCache[SpawnType.T] = Collect("info_player_terrorist");

        var dm = Collect("info_player_deathmatch");
        if (dm.Count == 0)
            dm = Collect("info_deathmatch_spawn");

        _globals.spawnCache[SpawnType.DM] = dm;

        /*
        _logger.LogInformation(
            $"[SpawnCache] CT={_globals.spawnCache[SpawnType.CT].Count}, " +
            $"T={_globals.spawnCache[SpawnType.T].Count}, " +
            $"DM={_globals.spawnCache[SpawnType.DM].Count}");
        */
    }

    private List<SpawnPointData> Collect(string classname)
    {
        var list = new List<SpawnPointData>();

        foreach (var ent in _core.EntitySystem.GetAllEntitiesByDesignerName<SpawnPoint>(classname))
        {
            if (ent == null || !ent.IsValid || !ent.IsValidEntity)
                continue;

            var Origin = ent.AbsOrigin;
            if (Origin == null)
                continue;

            var Rotation = ent.AbsRotation;
            if (Rotation == null)
                continue;


            list.Add(new SpawnPointData
            {
                Position = Origin.Value,
                Angle = Rotation.Value
            });
        }

        return list;
    }

    public List<SpawnPointData> GetSpawnPool(string config)
    {
        var pool = new List<SpawnPointData>();

        if (string.IsNullOrWhiteSpace(config))
            return pool;

        var types = config.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var type in types)
        {
            if (Enum.TryParse<SpawnType>(type, true, out var spawnType) &&
                _globals.spawnCache.TryGetValue(spawnType, out var list))
            {
                pool.AddRange(list);
            }
        }

        return pool;

    }

    public void KnockBackZombie(IPlayer attacker, IPlayer target, string inflictor, float force, bool isheadshot, HZPMainCFG CFG)
    {
 
        if (attacker == null || !attacker.IsValid)
            return;

        if (!inflictor.Equals("player", StringComparison.OrdinalIgnoreCase))
            return;

        if (target == null || !target.IsValid)
            return;

        if(!attacker.IsAlive || !target.IsAlive)
            return;

        _globals.IsZombie.TryGetValue(attacker.PlayerID, out bool attackerisZombie);
        _globals.IsZombie.TryGetValue(target.PlayerID, out bool targetisZombie);
        if (attackerisZombie || !targetisZombie)
            return;

        var targetpawn = target.PlayerPawn;
        if (targetpawn == null || !targetpawn.IsValid)
            return;

        var attackerpawn = attacker.PlayerPawn;
        if (attackerpawn == null || !attackerpawn.IsValid)
            return;

        _globals.IsSniper.TryGetValue(attacker.PlayerID, out bool IsSniper);
        _globals.IsSurvivor.TryGetValue(attacker.PlayerID, out bool IsSurvivor);
        _globals.IsHero.TryGetValue(attacker.PlayerID, out bool IsHero);

        QAngle attackerEye = attackerpawn.EyeAngles;
        attackerEye.ToDirectionVectors(out Vector vecKnockback, out _, out _);

        bool isOnGround = targetpawn.GroundEntity.IsValid;
        bool currentlyInAir = !isOnGround && targetpawn.AbsVelocity.Z != 0;
        bool isHero = (IsSniper || IsSurvivor || IsHero);

        float hitgroupsKnockback = isheadshot ? CFG.HumanKnockBackHeadMultiply : CFG.HumanKnockBackBodyMultiply;

        float airOrgroundKnock = currentlyInAir ? force * CFG.HumanKnockBackAirMultiply : force;

        float totalKnockback = hitgroupsKnockback * airOrgroundKnock;

        float finalKnock = isHero ? (totalKnockback * CFG.HumanHeroKnockBackMultiply) : totalKnockback;

        var pushVelocity = vecKnockback * finalKnock;

        var vel = targetpawn.AbsVelocity;

        targetpawn.Teleport(null, null, vel + pushVelocity);

    }

    public CParticleSystem? CreateParticleAtPos(CCSPlayerPawn pawn, Vector pos, string effectName)
    {
        if (pawn == null || !pawn.IsValid)
            return null;

        var particle = _core.EntitySystem.CreateEntityByDesignerName<CParticleSystem>("info_particle_system");
        if (!particle.IsValid || !particle.IsValidEntity)
            return null;


        particle.StartActive = true;
        particle.EffectName = effectName;
        particle.AcceptInput("Start", "");
        particle.DispatchSpawn();


        particle.Teleport(pos, QAngle.Zero, Vector.Zero);
        particle.AcceptInput("SetParent", "!activator", pawn, particle);
        return particle;
    }
    public void DrawExpandingRing(Vector position, float maxRadius, int R, int G, int B, int A, float duration = 0.4f, int segments = 16, float thickness = 18.0f)
    {
        CBeam?[] beams = new CBeam?[segments];
        float startTime = _core.Engine.GlobalVars.CurrentTime;
        for (int i = 0; i < segments; i++)
        {
            float angle = MathF.PI * 2 * i / segments;
            float nextAngle = MathF.PI * 2 * (i + 1) / segments;

            Vector start = new(
                position.X + MathF.Cos(angle),
                position.Y + MathF.Sin(angle),
                position.Z
            );

            Vector end = new(
                position.X + MathF.Cos(nextAngle),
                position.Y + MathF.Sin(nextAngle),
                position.Z
            );

            beams[i] = CreateLaser(
                start,
                end,
                new Color(R, G, B, A),
                thickness
            );
        }

        CancellationTokenSource? timer = null;

        timer = _core.Scheduler.RepeatBySeconds(0.01f, () =>
        {
            float now = _core.Engine.GlobalVars.CurrentTime;
            float progress = MathF.Min((now - startTime) / duration, 1.0f);
            float currentRadius = maxRadius * progress;

            for (int i = 0; i < segments; i++)
            {
                var beam = beams[i];
                if (beam is not { IsValid: true, IsValidEntity: true })
                    continue;

                float angle = MathF.PI * 2 * i / segments;
                float nextAngle = MathF.PI * 2 * (i + 1) / segments;

                Vector start = new(
                    position.X + currentRadius * MathF.Cos(angle),
                    position.Y + currentRadius * MathF.Sin(angle),
                    position.Z
                );

                Vector end = new(
                    position.X + currentRadius * MathF.Cos(nextAngle),
                    position.Y + currentRadius * MathF.Sin(nextAngle),
                    position.Z
                );

                TeleportLaser(beam, start, end);
            }

            if (progress >= 1.0f)
            {
                for (int i = 0; i < segments; i++)
                {
                    var beam = beams[i];
                    if (beam is not { IsValid: true, IsValidEntity: true })
                        continue;

                    beam.AcceptInput("Kill", 0);
                    beams[i] = null;
                }

                timer?.Cancel();
            }
        });
    }

    private CBeam? CreateLaser(Vector start, Vector end, SwiftlyS2.Shared.Natives.Color color, float width)
    {
        var beam = _core.EntitySystem.CreateEntityByDesignerName<CBeam>("beam");
        if (beam == null || !beam.IsValid || !beam.IsValidEntity)
            return null;

        beam.Render = color;
        beam.Width = width;
        beam.HaloScale = 3.0f;
        beam.Teleport(start, new QAngle(), new Vector(0, 0, 0));
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        beam.DispatchSpawn();

        return beam;
    }

    private void TeleportLaser(CBeam beam, Vector start, Vector end)
    {
        if (beam == null || !beam.IsValid || !beam.IsValidEntity) return;
        beam.Teleport(start, new QAngle(), new Vector(0, 0, 0));
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        beam.EndPosUpdated();
    }

    public void StartIgnite(IPlayer attacker, IPlayer zombie, float initialDmg, float burnDmg, float duration, string burnsound, float Volume)
    {
        if (attacker == null || !attacker.IsValid)
            return;

        if (zombie == null || !zombie.IsValid)
            return;

        if (attacker == zombie)
            return;

        int playerId = zombie.PlayerID;

        // 如果已经在燃烧，先清理旧的
        ClearPlayerBurn(playerId);

        // 创建粒子
        var pawn = zombie.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        ApplyDamage(attacker, zombie, initialDmg);

        var origin = pawn.AbsOrigin;
        if (origin == null)
            return;

        Vector offsetPos = new(origin.Value.X, origin.Value.Y, origin.Value.Z + 15);

        var particle = CreateParticleAtPos(pawn, offsetPos, "particles/burning_fx/env_fire_large.vpcf");
        if (particle == null || !particle.IsValid || !particle.IsValidEntity)
            return;

        // 持续伤害timer
        float startTime = _core.Engine.GlobalVars.CurrentTime;
        float lastSoundTime = startTime;
        var timer = _core.Scheduler.RepeatBySeconds(0.2f, () =>
        {
            if (zombie == null || !zombie.IsValid || pawn == null || !pawn.IsValid)
            {
                ClearPlayerBurn(playerId);
                return;
            }

            float elapsed = _core.Engine.GlobalVars.CurrentTime - startTime;
            if (elapsed >= duration)
            {
                ClearPlayerBurn(playerId);
                return;
            }

            if (particle == null || !particle.IsValid || !particle.IsValidEntity)
            {
                _globals.ActiveBurns.Remove(playerId);
                return;
            }


            ApplyDamage(attacker, zombie, burnDmg);

            if (_core.Engine.GlobalVars.CurrentTime - lastSoundTime >= 1.0f)
            {
                if (string.IsNullOrWhiteSpace(burnsound))
                    return;

                var sound = RandomSelectSound(burnsound);
                if (string.IsNullOrWhiteSpace(sound))
                    return;

                EmitSoundFormPlayer(zombie, sound, Volume);
                lastSoundTime = _core.Engine.GlobalVars.CurrentTime;
            }

        });

        _globals.ActiveBurns[playerId] = (particle, timer);
    }

    public void ClearAllBurns()
    {
        var burns = _globals.ActiveBurns.ToList();
        foreach (var pair in burns)
        {
            var burn = pair.Value;

            if (burn.particle != null && burn.particle.IsValid && burn.particle.IsValidEntity)
            {
                burn.particle.AcceptInput("kill", 0);
            }
            burn.timer?.Cancel();
        }
        _globals.ActiveBurns.Clear();
    }

    public void ClearPlayerBurn(int playerId)
    {
        if (_globals.ActiveBurns.TryGetValue(playerId, out var burn))
        {
            if (burn.particle != null && burn.particle.IsValid && burn.particle.IsValidEntity)
            {
                try
                {
                    burn.particle.AcceptInput("kill", 0);
                }
                catch
                {
                    _logger.LogInformation("删除出错!");
                }
            }

            burn.timer?.Cancel();
            _globals.ActiveBurns.Remove(playerId);
        }
    }
    public void ApplyDamage(IPlayer attacker, IPlayer target, float damageAmount, DamageTypes_t damageType = DamageTypes_t.DMG_BULLET)
    {
        if (attacker == null || !attacker.IsValid)
            return;

        if (target == null || !target.IsValid)
            return;

        var AttackerPawn = attacker.PlayerPawn;
        if (AttackerPawn == null || !AttackerPawn.IsValid)
            return;

        var TargetPawn = target.PlayerPawn;
        if (TargetPawn == null || !TargetPawn.IsValid)
            return;

        CBaseEntity inflictorEntity = AttackerPawn;
        CBaseEntity attackerEntity = AttackerPawn;
        CBaseEntity abilityEntity = AttackerPawn;


        var damageInfo = new CTakeDamageInfo(inflictorEntity, attackerEntity, abilityEntity, damageAmount, damageType);

        damageInfo.DamageForce = new SwiftlyS2.Shared.Natives.Vector(0, 0, 10f);

        var targetPos = TargetPawn.AbsOrigin;
        if (targetPos != null)
        {
            damageInfo.DamagePosition = targetPos.Value;
        }
        target.TakeDamage(damageInfo);
    }

    public COmniLight? CreateLight(Vector position, float range, int ColorR, int ColorG, int ColorB, int ColorA, string sound)
    {
        var light = _core.EntitySystem.CreateEntity<COmniLight>();
        if (light == null || !light.IsValid)
            return null;

        light.Enabled = true;
        light.DirectLight = 3;
        light.OuterAngle = 360f;
        light.ColorMode = 0;
        light.Shape = 0;

        light.LightStyleString = "None";
        light.Color = new Color(ColorR, ColorG, ColorB, ColorA);
        light.Brightness = 5f;
        light.Range = range;

        light.Teleport(position, null, null);

        light.DispatchSpawn();

        if (!string.IsNullOrEmpty(sound))
        {
            var sounds = new SwiftlyS2.Shared.Sounds.SoundEvent(sound, 1.0f, 1.0f);
            sounds.SourceEntityIndex = (int)light.Index;
            sounds.Recipients.AddAllPlayers();
            _core.Scheduler.NextTick(() =>
            {
                sounds.Emit();
            });
        }

        return light;

    }

    public void RemoveLight(uint lightIndex)
    {
        if (!_globals.activeLights.TryGetValue(lightIndex, out var light))
            return;


        if (light.IsValid && light.Entity != null && light.Entity.IsValid && light.IsValidEntity)
            light.AcceptInput("kill", 0);

        _globals.activeLights.Remove(lightIndex);
        _globals.lightTimers.Remove(lightIndex);
    }

    public void ClearAllLights()
    {
        foreach (var (lightIndex, timer) in _globals.lightTimers)
        {
            timer.Cancel();
        }
        _globals.lightTimers.Clear();

        foreach (var light in _globals.activeLights.Values)
        {
            if (light.IsValid)
                light.AcceptInput("kill", 0);
        }
        _globals.activeLights.Clear();
    }

    public void SetZombieFreezeOrStun(IPlayer player, float duration, string sound = "")
    {
        if (player == null || !player.IsValid)
            return;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return;
        if (controller.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid) return;

        var Id = player.PlayerID;

        // 合并时间：取最大值
        if (_globals.StopZombieTimers.TryGetValue(Id, out var existing))
        {
            duration = Math.Max(duration, existing);
        }

        _globals.StopZombieTimers[Id] = duration;

        var moveType = MoveType_t.MOVETYPE_INVALID;
        pawn.MoveType = moveType;
        pawn.ActualMoveType = moveType;
        pawn.MoveTypeUpdated();

        if (!string.IsNullOrEmpty(sound))
            EmitSoundFormPlayer(player, sound, 0.6f);

        _core.Scheduler.DelayBySeconds(duration, () =>
        {
            if (_globals.StopZombieTimers.TryGetValue(Id, out var current) && current <= duration)
            {
                _globals.StopZombieTimers.Remove(Id);
                var moveType = MoveType_t.MOVETYPE_WALK;
                pawn.MoveType = moveType;
                pawn.ActualMoveType = moveType;
                pawn.MoveTypeUpdated();
            }
        });
    }
    public void ClearFreezeStaten(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        _globals.StopZombieTimers.Remove(player.PlayerID);

        pawn.MoveType = MoveType_t.MOVETYPE_WALK;
        pawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
        pawn.MoveTypeUpdated();
    }

    public bool CheckIsGrenade(CBasePlayerWeapon activeWeapon)
    {
        string CustomName = activeWeapon.AttributeManager.Item.CustomName;
        if (CustomName == "FireGrenade"
            || CustomName == "IceGrenade"
            || CustomName == "LightGrenade"
            || CustomName == "TeleprotGrenade"
            || CustomName == "Incgrenade")
            return true;

        return false;
    }

    public void RemoveSHumanClass(int playerid)
    {
        _globals.IsSurvivor.Remove(playerid);
        _globals.IsSniper.Remove(playerid);
        _globals.IsHero.Remove(playerid);
    }

    public void RemoveSZombieClass(int playerid)
    {
        _globals.IsNemesis.Remove(playerid);
        _globals.IsAssassin.Remove(playerid);
    }

    public void SetAllDefaultModel(HZPMainCFG CFG)
    {
        string Default = "characters/models/ctm_st6/ctm_st6_variante.vmdl";
        string Custom = string.IsNullOrEmpty(CFG.HumandefaultModel) ? Default : CFG.HumandefaultModel;
        var allplayer = _core.PlayerManager.GetAllPlayers();
        foreach (var player in allplayer)
        {
            if (player == null || !player.IsValid)
                continue;

            _core.Scheduler.NextWorldUpdate(() =>
            {
                if (player != null && player.IsValid)
                {
                    var pawn = player.PlayerPawn;
                    if (pawn != null && pawn.IsValid)
                    {
                        SetPlayerModelFixed(pawn, Custom);
                    }
                }
            });

        }
        

        
    }
    public void SendChatToAllT(string key, params object[] args)
    {
        foreach (var player in _core.PlayerManager.GetAllPlayers())
        {
            if (player == null || !player.IsValid)
                continue;

            if(player.IsFakeClient)
                continue;

            player.SendMessage(MessageType.Chat, T(player, key, args));
        }
    }

    public void SendCenterToAllT(string key, params object[] args)
    {
        foreach (var player in _core.PlayerManager.GetAllPlayers())
        {
            if (player == null || !player.IsValid)
                continue;

            if (player.IsFakeClient)
                continue;

            player.SendMessage(MessageType.Center, T(player, key, args));
        }
    }

    

    public string T(IPlayer? player, string key, params object[] args)
    {
        if (player == null || !player.IsValid)
            return string.Format(key, args);

        var localizer = _core.Translation.GetPlayerLocalizer(player);
        return localizer[key, args];
    }

    public void CheckGrenadeSpawned(CEntityInstance entity)
    {
        if (entity == null || !entity.IsValid || !entity.IsValidEntity)
            return;

        var grenade = entity.As<CBaseCSGrenadeProjectile>();
        if (grenade == null || !grenade.IsValid || !grenade.IsValidEntity)
            return;

        if (!grenade.Thrower.IsValid || grenade.Thrower.Value == null || !grenade.Thrower.Value.IsValidEntity)
            return;

        var pawn = grenade.Thrower.Value;
        if (pawn == null || !pawn.IsValid)
            return;

        var player = _core.PlayerManager.GetPlayerFromPawn(pawn);
        if (player == null || !player.IsValid)
            return;

        _globals.IsZombie.TryGetValue(player.PlayerID, out bool IsZombie);

        string trailPath = "particles/ui/hud/ui_map_def_utility_trail.vpcf";
        string firePath = "particles/burning_fx/barrel_burning_trail.vpcf";
        string blackPath = "particles/environment/de_train/train_coal_dump_trails.vpcf";
        if (grenade.DesignerName.Equals("hegrenade_projectile", StringComparison.OrdinalIgnoreCase))
        {
            if (IsZombie)
            {
                var Trail = CreateParticleGlow(grenade, blackPath);
                if (Trail != null && Trail.IsValid && Trail.IsValidEntity)
                {
                    Trail.AcceptInput("FollowEntity", "!activator", grenade, Trail);
                }
            }
            else
            {
                var Trail = CreateParticleGlow(grenade, firePath);
                if (Trail != null && Trail.IsValid && Trail.IsValidEntity)
                {
                    Trail.AcceptInput("FollowEntity", "!activator", grenade, Trail);
                }
            }
        }

        var trail = CreateParticleGlow(grenade, trailPath);
        if (trail != null && trail.IsValid && trail.IsValidEntity)
        {
            trail.AcceptInput("FollowEntity", "!activator", grenade, trail);
        }
    }
    public CEnvParticleGlow? CreateParticleGlow(CBaseCSGrenadeProjectile grenade, string particles)
    {
        var entity = _core.EntitySystem.CreateEntity<CEnvParticleGlow>();
        if (entity == null || !entity.IsValid || !entity.IsValidEntity)
            return null;

        entity.StartActive = true;
        entity.EffectName = particles;
        entity.RenderMode = RenderMode_t.kRenderNormal;

        entity.DispatchSpawn();
        entity.AcceptInput("Start", 0);

        return entity;
    }


    public void SetPlayerModelFixed(CCSPlayerPawn pawn, string modelPath)
    {
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        if (string.IsNullOrWhiteSpace(modelPath))
            return;

        pawn.SetModel(modelPath);
        FixPlayerModelAnimations(pawn);
    }

    public void FixPlayerModelAnimations(CCSPlayerPawn pawn)
    {
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var originalMoveType = pawn.ActualMoveType;

        // 参考 CS2Fixes：只对正常可行走状态做修复
        if ((int)originalMoveType < (int)MoveType_t.MOVETYPE_WALK)
            return;

        var pawnHandle = _core.EntitySystem.GetRefEHandle(pawn);
        if (!pawnHandle.IsValid)
            return;

        var originalVelocity = pawn.AbsVelocity;

        // CS2Fixes 这里用的是 MOVETYPE_OBSOLETE。
        var tempMoveType = MoveType_t.MOVETYPE_OBSOLETE;

        pawn.Teleport(null, null, new Vector(0, 0, 0));
        pawn.MoveType = tempMoveType;
        pawn.ActualMoveType = tempMoveType;
        pawn.MoveTypeUpdated();

        _core.Scheduler.DelayBySeconds(0.02f, () =>
        {
            if (!pawnHandle.IsValid || pawnHandle.Value == null || !pawnHandle.Value.IsValid)
                return;

            var currentPawn = pawnHandle.Value;
            if (currentPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                return;

            currentPawn.MoveType = MoveType_t.MOVETYPE_WALK;
            currentPawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            currentPawn.MoveTypeUpdated();

            currentPawn.Teleport(null, null, originalVelocity);
        });
    }

    

}