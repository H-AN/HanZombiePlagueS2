using System.Security.AccessControl;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Helpers;
using SwiftlyS2.Shared.Misc;
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
    public void TVaccine(IPlayer player, int maxHealth, float initialSpeed, string models, string sound, float volume)
    {
        if (player == null || !player.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if(!IsZombie)
            return;

        _globals.IsAssassin.TryGetValue(Id, out bool IsAssassin);
        _globals.IsNemesis.TryGetValue(Id, out bool IsNemesis);
        if (IsAssassin || IsNemesis)
        {
            player.SendCenter(T(player, "ItemTVaccineError"));
            return;
        }

        _globals.IsZombie[Id] = false;
        player.SwitchTeam(Team.CT);
        ChangeKnife(player, false, false);
        SetFov(player, 90);
        ClearPlayerBurn(Id);

        SetPlayerModelFixed(pawn, models);

        pawn.MaxHealth = maxHealth;
        pawn.MaxHealthUpdated();
        pawn.Health = maxHealth;
        pawn.HealthUpdated();

        pawn.VelocityModifier = initialSpeed;
        pawn.VelocityModifierUpdated();

        ClearFreezeStaten(player);
        EmitSoundFormPlayer(player, sound, volume);
        HelperCheckRoundWin();
    }

    public void TVirusGrenade(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (!IsZombie)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var ws = pawn.WeaponServices;
        if (ws == null || !ws.IsValid)
            return;

        ws.DropWeaponBySlot(gear_slot_t.GEAR_SLOT_GRENADES);
        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var grenade = Is.GiveItem<CCSWeaponBase>("weapon_hegrenade");
        if (grenade == null || !grenade.IsValid)
            return;

        grenade.Entity!.Name = "TVirusGrenade";
        grenade.AcceptInput("ChangeSubclass", "44");
        grenade.AttributeManager.Item.Initialized = true;
        grenade.AttributeManager.Item.ItemDefinitionIndex = 44;
        grenade.AttributeManager.Item.CustomName = "TVirusGrenade";
        grenade.AttributeManager.Item.CustomNameOverride = "TVirusGrenade";
        grenade.AttributeManager.Item.CustomNameUpdated();
    }

    public void GiveScbaSuit(IPlayer player, string sound)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        _globals.ScbaSuit.TryGetValue(Id, out bool IsHaveScbaSuit);
        if (IsHaveScbaSuit)
        {
            player.SendMessage(MessageType.Chat, T(player, "ItemSCBASuitAlready"));
            return;
        }

        _globals.ScbaSuit[Id] = true;
        EmitSoundFormPlayer(player, sound, 1.0f);
    }

    public void RemoveScbaSuit(IPlayer player, string sound)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.ScbaSuit.TryGetValue(Id, out bool IsHaveScbaSuit);
        if(!IsHaveScbaSuit)
            return;

        _globals.ScbaSuit[Id] = false;
        EmitSoundFormPlayer(player, sound, 1.0f);
        player.SendMessage(MessageType.Chat, T(player, "ItemSCBASuitBroken"));
    }

    public void SetGodState(IPlayer player, float time)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.GodState.TryGetValue(Id, out bool IsGodState);
        if (IsGodState)
        {
            player.SendMessage(MessageType.Chat, T(player, "ItemGodModeAlready"));
            return;
        }

        _globals.GodState[Id] = true;
        _core.Scheduler.DelayBySeconds(time, () => 
        {
            RemoveGodState(player);
        });
    }

    public void RemoveGodState(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.GodState.TryGetValue(Id, out bool IsGodState);
        if (!IsGodState)
            return;

        _globals.GodState[Id] = false;
        player.SendMessage(MessageType.Chat, T(player, "ItemGodModeEnd"));
    }


    public void AddHealth(IPlayer player, int MaxHealth, int valve, string sound)
    {
        if (player == null || !player.IsValid)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return;

        if(controller.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var currentHealth = pawn.Health;
        var newHealth = currentHealth + valve;

        if(currentHealth >= MaxHealth)
            return;

        pawn.Health = newHealth;
        pawn.HealthUpdated();

        EmitSoundFormPlayer(player, sound, 1.0f);
    }

    public void SetInfiniteAmmoState(IPlayer player, float time)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.InfiniteAmmoState.TryGetValue(Id, out bool IsInfiniteAmmoState);
        if (IsInfiniteAmmoState)
        {
            player.SendMessage(MessageType.Chat, T(player, "ItemInfiniteAmmoAlready"));
            return;
        }

        _globals.InfiniteAmmoState[Id] = true;
        _core.Scheduler.DelayBySeconds(time, () =>
        {
            RemoveInfiniteAmmo(player);
        });
    }

    public void RemoveInfiniteAmmo(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.InfiniteAmmoState.TryGetValue(Id, out bool IsInfiniteAmmoState);
        if (!IsInfiniteAmmoState)
            return;

        _globals.InfiniteAmmoState[Id] = false;
        player.SendMessage(MessageType.Chat, T(player, "ItemInfiniteAmmoEnd"));
    }

    

    public void HelperCheckRoundWin()
    {
        var allPlayers = _core.PlayerManager.GetAlive();
        int zombieCount = 0;
        int humanCount = 0;

        foreach (var p in allPlayers)
        {
            if (p == null || !p.IsValid)
                continue;

            if (p.PlayerPawn == null || !p.PlayerPawn.IsValid)
                continue;

            if (!p.Controller.PawnIsAlive)
                continue;

            var Id = p.PlayerID;
            _globals.IsZombie.TryGetValue(Id, out bool IsZombie);

            if (IsZombie)
                zombieCount++;
            else
                humanCount++;
        }

        if (zombieCount == 0)
            helpershumanwin();
        else if (humanCount == 0)
            helperszombiewin();
    }
    public void helpershumanwin()
    {
        _globals.GameStart = false;

        if (_globals.RoundVoxGroup != null)
        {
            if (!string.IsNullOrWhiteSpace(_globals.RoundVoxGroup.HumanWinVox))
            {
                var sound = RandomSelectSound(_globals.RoundVoxGroup.HumanWinVox);
                if (sound != null)
                {
                    EmitSoundToAll(sound, _globals.RoundVoxGroup.Volume);
                }
            }
        }
        SendCenterToAllT("ServerGameHumanWin");
        SetTeamScore(Team.CT);
        TerminateRound(RoundEndReason.CTsWin, 5.0f);
    }
    public void helperszombiewin()
    {
        _globals.GameStart = false;

        if (_globals.RoundVoxGroup != null)
        {
            if (!string.IsNullOrWhiteSpace(_globals.RoundVoxGroup.ZombieWinVox))
            {
                var sound = RandomSelectSound(_globals.RoundVoxGroup.ZombieWinVox);
                if (sound != null)
                {
                    EmitSoundToAll(sound, _globals.RoundVoxGroup.Volume);
                }
            }
        }
        SendCenterToAllT("ServerGameZombieWin");
        SetTeamScore(Team.T);
        TerminateRound(RoundEndReason.TerroristsWin, 5.0f);
    }

    public void GiveFireGrenade(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var ws = pawn.WeaponServices;
        if (ws == null || !ws.IsValid)
            return;

        ws.DropWeaponBySlot(gear_slot_t.GEAR_SLOT_GRENADES);
        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var grenade = Is.GiveItem<CCSWeaponBase>("weapon_hegrenade");
        if (grenade == null || !grenade.IsValid)
            return;

        grenade.Entity!.Name = "FireGrenade";
        grenade.AcceptInput("ChangeSubclass", "44");
        grenade.AttributeManager.Item.Initialized = true;
        grenade.AttributeManager.Item.ItemDefinitionIndex = 44;
        grenade.AttributeManager.Item.CustomName = "FireGrenade";
        grenade.AttributeManager.Item.CustomNameOverride = "FireGrenade";
        grenade.AttributeManager.Item.CustomNameUpdated();
    }

    public void GiveFreezeGrenade(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;


        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var grenade = Is.GiveItem<CCSWeaponBase>("weapon_smokegrenade");
        if (grenade == null || !grenade.IsValid)
            return;

        grenade.Entity!.Name = "FreezeGrenade";
        grenade.AcceptInput("ChangeSubclass", "45");
        grenade.AttributeManager.Item.Initialized = true;
        grenade.AttributeManager.Item.ItemDefinitionIndex = 45;
        grenade.AttributeManager.Item.CustomName = "FreezeGrenade";
        grenade.AttributeManager.Item.CustomNameOverride = "FreezeGrenade";
        grenade.AttributeManager.Item.CustomNameUpdated();
    }

    public void GiveLightGrenade(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var grenade = Is.GiveItem<CCSWeaponBase>("weapon_flashbang");
        if (grenade == null || !grenade.IsValid)
            return;

        grenade.Entity!.Name = "LightGrenade";
        grenade.AcceptInput("ChangeSubclass", "43");
        grenade.AttributeManager.Item.Initialized = true;
        grenade.AttributeManager.Item.ItemDefinitionIndex = 43;
        grenade.AttributeManager.Item.CustomName = "LightGrenade";
        grenade.AttributeManager.Item.CustomNameOverride = "LightGrenade";
        grenade.AttributeManager.Item.CustomNameUpdated();
    }

    public void GiveTeleprotGrenade(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var grenade = Is.GiveItem<CCSWeaponBase>("weapon_decoy");
        if (grenade == null || !grenade.IsValid)
            return;

        grenade.Entity!.Name = "TeleprotGrenade";
        grenade.AcceptInput("ChangeSubclass", "47");
        grenade.AttributeManager.Item.Initialized = true;
        grenade.AttributeManager.Item.ItemDefinitionIndex = 47;
        grenade.AttributeManager.Item.CustomName = "TeleprotGrenade";
        grenade.AttributeManager.Item.CustomNameOverride = "TeleprotGrenade";
        grenade.AttributeManager.Item.CustomNameUpdated();
    }

    public void GiveIncGrenade(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return;

        var Id = player.PlayerID;

        _globals.IsZombie.TryGetValue(Id, out bool IsZombie);
        if (IsZombie)
            return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            return;

        var Is = pawn.ItemServices;
        if (Is == null || !Is.IsValid)
            return;

        var grenade = Is.GiveItem<CCSWeaponBase>("weapon_incgrenade");
        if (grenade == null || !grenade.IsValid)
            return;

        grenade.Entity!.Name = "Incgrenade";
        grenade.AcceptInput("ChangeSubclass", "48");
        grenade.AttributeManager.Item.Initialized = true;
        grenade.AttributeManager.Item.ItemDefinitionIndex = 48;
        grenade.AttributeManager.Item.CustomName = "Incgrenade";
        grenade.AttributeManager.Item.CustomNameOverride = "Incgrenade";
        grenade.AttributeManager.Item.CustomNameUpdated();
    }



}