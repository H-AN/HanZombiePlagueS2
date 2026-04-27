namespace HanZombiePlagueS2;

public enum GameModeType
{
    Normal,
    NormalInfection,
    MultiInfection,
    Nemesis,
    Survivor,
    Swarm,
    Plague,
    Assassin,
    Sniper,
    AVS,
    Hero
}
public class GameModeConfig
{
    public bool Enable { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    public int Weight { get; set; } = 100;
    public bool EnableInfiniteClipMode { get; set; } = true;
    public bool ZombieCanReborn { get; set; } = true;
    
}

public class NemesisModeConfig : GameModeConfig
{
    public string NemesisNames { get; set; } = string.Empty;
}

public class NormalInfectionModeConfig : GameModeConfig
{
    public string MotherZombieNames { get; set; } = string.Empty;
    public int MotherZombieCount { get; set; } = 1;
}
public class MultiInfectionModeConfig : GameModeConfig
{
    public string MotherZombieNames { get; set; } = string.Empty;
    public int MotherZombieCount { get; set; } = 5;
}

public class SurvivorModeConfig : GameModeConfig
{
    public string SurvivorNames { get; set; } = string.Empty;
    public int SurvivorHealth { get; set; } = 1000;
    public float SurvivorSpeed { get; set; } = 3.0f;
    public float SurvivorGravity { get; set; } = 3.0f;
    public float SurvivorDamage { get; set; } = 5.0f;
    public string SurvivorWeapon{ get; set; } = string.Empty;
    public string CustomWeaponName { get; set; } = string.Empty;
    public string ModelsPath { get; set; } = string.Empty;
}

public class AssassinModeConfig : GameModeConfig
{
    public string AssassinNames { get; set; } = string.Empty;
    public float InvisibilityDist { get; set; } = 200.0f;
}

public class SniperModeConfig : GameModeConfig
{
    public string SniperNames { get; set; } = string.Empty;
    public int SniperHealth { get; set; } = 500;
    public float SniperSpeed { get; set; } = 3.0f;
    public float SniperGravity { get; set; } = 3.0f;
    public float SniperDamage { get; set; } = 10.0f;
    public string SniperWeapon { get; set; } = string.Empty;

    public string CustomWeaponName { get; set; } = string.Empty;
    public string ModelsPath { get; set; } = string.Empty;
}

public class PlagueModeConfig : GameModeConfig
{
    public string NemesisNames { get; set; } = string.Empty;
    public string SurvivorNames { get; set; } = string.Empty;
}

public class AVSConfig : GameModeConfig
{
    public string AssassinNames { get; set; } = string.Empty;
    public string SniperNames { get; set; } = string.Empty;
}

public class HeroConfig : GameModeConfig
{
    public int HeroCount { get; set; }
    public string HeroNames { get; set; } = string.Empty;
    public int HeroHealth { get; set; } = 500;
    public float HeroSpeed { get; set; } = 3.0f;
    public float HeroGravity { get; set; } = 3.0f;
    public float HeroDamage { get; set; } = 10.0f;
    public string ModelsPath { get; set; } = string.Empty;

}
public class HZPMainCFG
{
    public float RoundReadyTime { get; set; } = 25f;
    public float RoundTime { get; set; } = 3;

    public NormalInfectionModeConfig NormalInfection { get; set; } = new();
    public MultiInfectionModeConfig MultiInfection { get; set; } = new();
    public NemesisModeConfig Nemesis { get; set; } = new();
    public SurvivorModeConfig Survivor { get; set; } = new();
    public GameModeConfig Swarm { get; set; } = new();
    public PlagueModeConfig Plague { get; set; } = new();
    public AssassinModeConfig Assassin { get; set; } = new();
    public SniperModeConfig Sniper { get; set; } = new();
    public AVSConfig AVS { get; set; } = new();
    public HeroConfig Hero { get; set; } = new();

    public string HumandefaultModel { get; set; } = string.Empty;
    public int HumanMaxHealth { get; set; } = 225;
    public bool EnableDamageHud { get; set; } = true;
    public float HumanInitialSpeed { get; set; } = 1.0f;
    public float HumanInitialGravity { get; set; } = 0.8f;
    public float HumanKnockBackHeadMultiply { get; set; } = 2.0f;
    public float HumanKnockBackBodyMultiply { get; set; } = 1.0f;
    public float HumanKnockBackGroundMultiply { get; set; } = 1.0f;
    public float HumanKnockBackAirMultiply { get; set; } = 0.5f;
    public float HumanHeroKnockBackMultiply { get; set; } = 1.0f;
    public bool EnableInfiniteReserveAmmo { get; set; } = true;
    public bool EnableWeaponNoRecoil { get; set; } = true;
    public string HumanSpawnPoints { get; set; } = string.Empty;
    public string ZombieSpawnPoints { get; set; } = string.Empty;
    public float KnockZombieForce { get; set; } = 250f;
    public float StunZombieTime { get; set; } = 0.1f;
    public string TVaccineSound { get; set; } = string.Empty;
    public float TVirusGrenadeRange { get; set; } = 300.0f;
    public bool TVirusCanInfectHero { get; set; } = true;
    public string TVirusGrenadeSound { get; set; } = string.Empty;
    public string AddHealthSound { get; set; } = string.Empty;
    public bool FireGrenade { get; set; } = true;
    public bool SpawnGiveFireGrenade { get; set; } = true;
    public bool SpawnGiveSpecialGrenadesToBots { get; set; } = false;
    public float FireGrenadeRange { get; set; } = 300.0f;
    public float FireGrenadeDmg { get; set; } = 500f;
    public float FireDmg { get; set; } = 5f;
    public float FireGrenadeDuration { get; set; } = 8f;
    public string FireGrenadeSound { get; set; } = string.Empty;
    public bool SpawnGiveIncGrenade { get; set; } = true;
    public bool LightGrenade { get; set; } = true;
    public bool SpawnGiveLightGrenade { get; set; } = true;
    public float LightGrenadeRange { get; set; } = 1000f;
    public float LightGrenadeDuration { get; set; } = 30f;
    public string LightGrenadeSound { get; set; } = string.Empty;
    public bool FreezeGrenade { get; set; } = true;
    public bool SpawnGiveFreezeGrenade { get; set; } = true;
    public float FreezeGrenadeRange { get; set; } = 300;
    public float FreezeGrenadeDuration { get; set; } = 6f;
    public string FreezeGrenadeSound { get; set; } = string.Empty;
    public bool TelportGrenade { get; set; } = true;
    public bool SpawnGiveTelportGrenade { get; set; } = true;
    public bool CanUseScbaSuit { get; set; } = true;
    public string ScbaSuitGetSound { get; set; } = string.Empty;
    public string ScbaSuitBrokenSound { get; set; } = string.Empty;
    public string ZombieClassCommand { get; set; } = "sw_zclass";
    public string AdminMenuItemCommand { get; set; } = "sw_zmenu";
    public string AdminMenuPermission { get; set; } = "";
    public string AmbSound { get; set; } = string.Empty;
    public float AmbSoundLoopTime { get; set; } = 60.0f;
    public float AmbSoundVolume { get; set; } = 0.6f;
    public string PrecacheAmbSound { get; set; } = string.Empty;

}
