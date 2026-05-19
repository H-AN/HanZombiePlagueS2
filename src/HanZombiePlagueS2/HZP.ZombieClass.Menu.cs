using System.Drawing;
using System.Reflection.Emit;
using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Cecil.Cil;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.SteamAPI;
using static HanZombiePlagueS2.HZPZombieClassCFG;

namespace HanZombiePlagueS2;

public class HZPZombieClassMenu
{
    private readonly ILogger<HZPZombieClassMenu> _logger;
    private readonly ISwiftlyCore _core;
    private readonly HZPMenuHelper _menuhelper;
    private readonly IOptionsMonitor<HZPZombieClassCFG> _zombieClassCFG;
    private readonly PlayerZombieState _zombieState;
    private readonly HZPHelpers _helpers;
    private readonly HanZombiePlagueAPI _api;

    public HZPZombieClassMenu(ISwiftlyCore core, ILogger<HZPZombieClassMenu> logger,
        HZPMenuHelper menuHelper, IOptionsMonitor<HZPZombieClassCFG> zombieClassCFG,
        PlayerZombieState zombieState, HZPHelpers helpers, HanZombiePlagueAPI api)
    {
        _core = core;
        _logger = logger;
        _menuhelper = menuHelper;
        _zombieClassCFG = zombieClassCFG;
        _zombieState = zombieState;
        _helpers = helpers;
        _api = api;
    }

    public IMenuAPI OpenZombieClassMenu(IPlayer player)
    {
        var main = _core.MenusAPI.CreateBuilder();
        IMenuAPI menu = _menuhelper.CreateMenu(_helpers.T(player, "ZClassMenu"));

        var Id = player.PlayerID;
        var steamId = player.SteamID;
        var currentPreference = _zombieState.GetPlayerPreference(Id, steamId);

        menu.AddOption(new TextMenuOption(HtmlGradient.GenerateGradientText(
            _helpers.T(player, "ZClassMenuSelect"),
            Color.Red, Color.LightBlue, Color.Red),
            updateIntervalMs: 500, pauseIntervalMs: 100)
        {
            TextStyle = MenuOptionTextStyle.ScrollLeftLoop
        });

        string randomButtonText = $"{_helpers.T(player, "ZClassMenuRandomSelect")} {(currentPreference?.Preference == ZombiePreference.Random ? "✓" : "")}";
        var RandomButton = new ButtonMenuOption(randomButtonText)
        {
            TextStyle = MenuOptionTextStyle.ScrollLeftLoop,
            CloseAfterClick = true
        };
        RandomButton.Tag = "extend";

        RandomButton.Click += async (_, args) =>
        {
            RunMenuAction(args.Player, clicker =>
            {
                if (_api != null)
                    _api.NotifyUpdatePreferenceFromMenu(clicker.PlayerID, clicker.SteamID, null);

                clicker.SendMessage(MessageType.Chat, _helpers.T(clicker, "ZClassMenuRandomSelectInfo"));
            });
        };

        menu.AddOption(RandomButton);

        var zombieConfig = _zombieClassCFG.CurrentValue;
        var zombieClasses = zombieConfig.ZombieClassList;

        if (zombieClasses != null && zombieClasses.Count > 0)
        {
            foreach (var Cfg in zombieClasses)
            {
                string buttonText = $"{Cfg.Name} {(currentPreference?.Preference == ZombiePreference.Fixed && currentPreference.FixedZombieName == Cfg.Name ? "✓" : "")}";

                var Button = new ButtonMenuOption(buttonText)
                {
                    TextStyle = MenuOptionTextStyle.ScrollLeftLoop,
                    CloseAfterClick = true
                };
                Button.Tag = "extend";

                Button.Click += async (_, args) =>
                {
                    RunMenuAction(args.Player, clicker =>
                    {
                        if (_api != null)
                            _api.NotifyUpdatePreferenceFromMenu(clicker.PlayerID, clicker.SteamID, Cfg.Name);

                        clicker.SendMessage(MessageType.Chat, $"{_helpers.T(clicker, "ZClassMenuSelectInfo")} {Cfg.Name}");
                    });
                };

                menu.AddOption(Button);
            }
        }

        _core.MenusAPI.OpenMenuForPlayer(player, menu);
        return menu;
    }

    private void RunMenuAction(IPlayer clicker, Action<IPlayer> action)
    {
        _core.Scheduler.NextTick(() =>
        {
            if (clicker == null || !clicker.IsValid)
                return;

            action(clicker);
        });
    }

}
