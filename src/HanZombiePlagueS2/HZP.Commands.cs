using System.Numerics;
using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Cecil.Cil;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SteamAPI;

namespace HanZombiePlagueS2;

public class HZPCommands
{
    private readonly ILogger<HZPCommands> _logger;
    private readonly ISwiftlyCore _core;
    private readonly HZPServices _services;
    private readonly IOptionsMonitor<HZPMainCFG> _mainCFG;
    private readonly HZPGlobals _globals;
    private readonly HZPZombieClassMenu _hZPZombieClassMenu;
    private readonly HZPAdminItemMenu _hZPAdminItemMenu;
    private readonly HZPHelpers _helpers;

    public HZPCommands(ISwiftlyCore core, ILogger<HZPCommands> logger,
        HZPServices services, IOptionsMonitor<HZPMainCFG> mainCFG,
        HZPGlobals globals, HZPAdminItemMenu hZPAdminItemMenu,
        HZPZombieClassMenu hZPZombieClassMenu, HZPHelpers helpers)
    {
        _core = core;
        _logger = logger;
        _services = services;
        _mainCFG = mainCFG;
        _globals = globals;
        _hZPAdminItemMenu = hZPAdminItemMenu;
        _hZPZombieClassMenu = hZPZombieClassMenu;
        _helpers = helpers;
    }

    public void MenuCommands()
    {
        var CFG = _mainCFG.CurrentValue;
        _core.Command.RegisterCommand(CFG.ZombieClassCommand, SelectZombieClass, true);

        _core.Command.RegisterCommand(CFG.AdminMenuItemCommand, UseItemMenu, true);
    }
    public void SelectZombieClass(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid) 
            return;

        _hZPZombieClassMenu.OpenZombieClassMenu(player);

    }

    public void UseItemMenu(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;


        if (!HasAdminMenuPermission(player))
        {
            player.SendMessage(MessageType.Chat, _helpers.T(player, "NoPermission"));
            return;
        }
            

        _hZPAdminItemMenu.OpenAdminItemMenu(player);
    }

    private bool HasAdminMenuPermission(IPlayer player)
    {
        if (player == null || !player.IsValid)
            return false;

        ulong steamId = player.SteamID;
        if (steamId == 0)
            return false;

        var permString = _mainCFG.CurrentValue.AdminMenuPermission;

        if (string.IsNullOrWhiteSpace(permString))
            return true;

        foreach (var perm in permString.Split(','))
        {
            var p = perm.Trim();
            if (p.Length == 0)
                continue;

            if (_core.Permission.PlayerHasPermission(steamId, p))
                return true;
        }

        return false;
    }

    public void RoundCvar()
    {
        var CFG = _mainCFG.CurrentValue;
        _core.Engine.ExecuteCommand("mp_randomspawn 1");
        _core.Engine.ExecuteCommand($"mp_roundtime_hostage {CFG.RoundTime}");
        _core.Engine.ExecuteCommand($"mp_roundtime_defuse {CFG.RoundTime}");
        _core.Engine.ExecuteCommand($"mp_roundtime {CFG.RoundTime}");
        _core.Engine.ExecuteCommand("mp_give_player_c4 0");
        _core.Engine.ExecuteCommand("mp_autoteambalance 0");
        _core.Engine.ExecuteCommand("mp_limitteams 0");
        _core.Engine.ExecuteCommand("sv_legacy_jump 1");

    }

    public void ServerCvar()
    {
        
        _core.Engine.ExecuteCommand("mp_randomspawn 1");
        _core.Engine.ExecuteCommand("mp_roundtime_hostage 3");
        _core.Engine.ExecuteCommand("mp_roundtime_defuse 3");
        _core.Engine.ExecuteCommand("mp_roundtime 3");
        _core.Engine.ExecuteCommand("bot_quota_mode fill");
        _core.Engine.ExecuteCommand("bot_quota 20");
        _core.Engine.ExecuteCommand("mp_ignore_round_win_conditions 1");
        _core.Engine.ExecuteCommand("bot_join_after_player 1");
        _core.Engine.ExecuteCommand("bot_chatter off");
        _core.Engine.ExecuteCommand("mp_autokick 0");
        _core.Engine.ExecuteCommand("mp_round_restart_delay 0");
        _core.Engine.ExecuteCommand("mp_autoteambalance 0");
        _core.Engine.ExecuteCommand("mp_startmoney 16000");
        _core.Engine.ExecuteCommand("mp_autoteambalance 0");
        _core.Engine.ExecuteCommand("mp_limitteams 0");
        _core.Engine.ExecuteCommand("sv_legacy_jump 1");
    }
    public void Command()
    {
        _core.Command.RegisterCommand("jointeam", RegisterJoin, true);
        _core.Command.HookClientCommand(OnJoinTeam);

    }

    public void RegisterJoin(ICommandContext context){
    }


    public HookResult OnJoinTeam(int playerId, string commandLine)
    {
        IPlayer? player = _core.PlayerManager.GetPlayer(playerId);
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        _globals.IsZombie.TryGetValue(player.PlayerID, out bool IsZombie);
        if (!player.IsFakeClient && !IsZombie)
        {
            if (commandLine.StartsWith("jointeam 2"))
            {
                player.SwitchTeam(Team.CT);
                _core.Scheduler.DelayBySeconds(1.0f, () =>
                {
                    _services.JoinTeamCheck(player);
                });
            }
            else if (commandLine.StartsWith("jointeam 3"))
            {
                player.SwitchTeam(Team.CT);
                _core.Scheduler.DelayBySeconds(1.0f, () =>
                {
                    _services.JoinTeamCheck(player);
                });

            }
            else if (commandLine.StartsWith("jointeam 1"))
            {
                player.SwitchTeam(Team.CT);
                _core.Scheduler.DelayBySeconds(1.0f, () =>
                {
                    _services.JoinTeamCheck(player);
                });
                return HookResult.Stop;
            }

        }
        return HookResult.Continue;
    }


}