using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using static HanZombiePlagueS2.HZPZombieClassCFG;


namespace HanZombiePlagueS2;

public class HZPGlobals
{
    public bool ServerIsEmpty = true;
    public bool GameStart { get; set; }
    public bool SafeRoundStart { get; set; }
    public bool GameInfiniteClipMode { get; set; }
    public bool IsheroSetup { get; set; }
    public int Countdown { get; set; }

    public bool[] InSwing { get; } = new bool[65];

    public HZPVoxCFG.RoundVox? RoundVoxGroup = null;

    public Dictionary<int, bool> IsZombie = new Dictionary<int, bool>();

    public Dictionary<int, bool> IsMother = new Dictionary<int, bool>();
    public Dictionary<int, bool> IsSurvivor = new Dictionary<int, bool>();
    public Dictionary<int, bool> IsSniper = new Dictionary<int, bool>();
    public Dictionary<int, bool> IsNemesis = new Dictionary<int, bool>();
    public Dictionary<int, bool> IsAssassin = new Dictionary<int, bool>();
    public Dictionary<int, bool> IsHero = new Dictionary<int, bool>();

    public CancellationTokenSource? g_hRoundEndTimer { get; set; } = null;
    public CancellationTokenSource? g_hCountdown { get; set; } = null;

    public Dictionary<int, ZombieIdleState> g_ZombieIdleStates = new();
    public CancellationTokenSource? g_IdleTimer { get; set; } = null;

    public Dictionary<IPlayer, (int endTick, int fallEndTick, Vector originalVelocity)> jumpBoostState = new();

    public Dictionary<int, ZombieRegenState> g_ZombieRegenStates = new();

    public CancellationTokenSource? g_ZombieRegenTimer = null;

    public CancellationTokenSource? g_hAmbMusic { get; set; } = null;

    public Dictionary<int, bool> g_IsInvisible = new();

    public Dictionary<CCSPlayerController, GlowEntity> GlowEntity = new Dictionary<CCSPlayerController, GlowEntity>();

    public CancellationTokenSource? AssassinTimer;

    public Dictionary<int, bool> ThrowerIsZombie = new();

    public Dictionary<int, (CParticleSystem particle, CancellationTokenSource timer)> ActiveBurns = new();

    public Dictionary<uint, COmniLight> activeLights = new Dictionary<uint, COmniLight>();
    public Dictionary<uint, CancellationTokenSource> lightTimers = new Dictionary<uint, CancellationTokenSource>();

    public readonly Dictionary<SpawnType, List<SpawnPointData>> spawnCache= new();

    public Dictionary<int, float> StopZombieTimers = new();

    public Dictionary<int, bool> ScbaSuit = new Dictionary<int, bool>();
    public Dictionary<int, bool> GodState = new Dictionary<int, bool>();
    public Dictionary<int, bool> InfiniteAmmoState = new Dictionary<int, bool>();

    public readonly HashSet<string> LegacyZombieCustomNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "TVirusGrenade"
    };

    public readonly HashSet<string> LegacyHumanCustomNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "FireGrenade",
        "FreezeGrenade",
        "LightGrenade",
        "TeleprotGrenade",
        "Incgrenade"
    };



}
public class ZombieRegenState
{
    public int PlayerID;
    public int RegenAmount;       // 每次回血量
    public float RegenInterval;   // 间隔秒数
    public float NextRegenTime;   // 下一次回血时间戳（秒）
}

public class ZombieIdleState
{
    public int PlayerID;
    public float IdleInterval;   // 间隔秒数
    public float NextIdleTime;   // 下一次Idle时间
}

public enum SpawnType
{
    CT,
    T,
    DM
}
public struct SpawnPointData
{
    public Vector Position;
    public QAngle Angle;
}

public class GlowEntity
{
    public CBaseModelEntity? Relay { get; set; } = null;
    public CBaseModelEntity? Glow { get; set; } = null;

}


