using System.Numerics;
using System.Security.Principal;
using HanZombiePlagueS2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.SchemaDefinitions;
using static Dapper.SqlMapper;

namespace ZPAPITEST;

[PluginMetadata(Id = "ZPAPITEST",
    Version = "1.0.0",
    Name = "ZPAPITEST",
    Author = "han",
    Description = "test.")]

/*
 * 使用api需要将 HanZombiePlagueAPI.dll 放在插件同目录下 一起编译 /To use the API, place HanZombiePlagueAPI.dll  in the same directory as the plugin to Compile.
 * 需要在csproj内新增 / Need to add to csproj :
 * 
 * <ItemGroup> 
 * <Reference Include="HanZombiePlagueS2"> 
 * <HintPath>HanZombiePlagueAPI.dll
 * </HintPath> 
 * </Reference> 
 * </ItemGroup>
 */


public partial class ZPAPITEST(ISwiftlyCore core) : BasePlugin(core)
{
    private IHanZombiePlagueAPI? _zpApi; //创建api实例, Create an API instance

    
    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        if (interfaceManager.HasSharedInterface("HanZombiePlague")) //获取api  Get API
        {
            _zpApi = interfaceManager.GetSharedInterface<IHanZombiePlagueAPI>("HanZombiePlague");  //获取api  Get API
            Core.Logger.LogInformation($"[External] 成功获取 API/Successfully obtained API，Hash: {_zpApi.GetHashCode()}");

            if (_zpApi == null)
                return;

            _zpApi.HZP_OnGameStart += (bool gamestart) =>
            {
                Core.Logger.LogInformation("游戏开始事件触发 / Game start event triggered"); 
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 游戏开始了！游戏开始状态 / The game has started! (Game start status) {gamestart}");
            };

            _zpApi.HZP_OnPlayerInfect += (IPlayer attacker, IPlayer player, bool grenade, string zombieName) =>
            {
                Core.Logger.LogInformation("开始/start OnInfect");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 玩家/player {attacker.Controller.PlayerName} 感染了/Infect {player.Controller.PlayerName}");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 手雷感染/grenade Infect {grenade} 感染为/Infect to  {zombieName} ");
                Core.Logger.LogInformation("OnInfect 结束/end");
            };

            _zpApi.HZP_OnMotherZombieSelected += (IPlayer player) =>
            {
                Core.Logger.LogInformation("母体丧尸选择事件触发/Mother Zombie Selection Event Triggered");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 玩家/player {player.Controller.PlayerName} 被选为母体丧尸/Selected as the Mother Zombie");
            };

            _zpApi.HZP_OnNemesisSelected += (IPlayer player) =>
            {
                Core.Logger.LogInformation("复仇之神选择事件触发/Nemesis Selection Event Triggered");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 玩家/player {player.Controller.PlayerName} 被选为复仇之神/Selected as the Nemesis");
            };

            _zpApi.HZP_OnAssassinSelected += (IPlayer player) =>
            {
                Core.Logger.LogInformation("暗杀者选择事件触发/Assassin Selection Event Triggered");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 玩家/player {player.Controller.PlayerName} 被选为暗杀者/Selected as the Assassin");
            };

            _zpApi.HZP_OnHeroSelected += (IPlayer player) =>
            {
                Core.Logger.LogInformation("英雄选择事件触发/Hero Selection Event Triggered");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 玩家/player {player.Controller.PlayerName} 被选为英雄/Selected as the Hero");
            };

            _zpApi.HZP_OnSurvivorSelected += (IPlayer player) =>
            {
                Core.Logger.LogInformation("幸存者选择事件触发/Survivor Selection Event Triggered");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 玩家/player {player.Controller.PlayerName} 被选为幸存者/Selected as the Survivor");
            };

            _zpApi.HZP_OnSniperSelected += (IPlayer player) =>
            {
                Core.Logger.LogInformation("狙击手选择事件触发/Sniper Selection Event Triggered");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 玩家/player {player.Controller.PlayerName} 被选为狙击手/Selected as the Sniper");
            };

            _zpApi.HZP_OnHumanWin += (bool result) =>
            {
                Core.Logger.LogInformation("胜负事件触发/Victory/Defeat Event Triggered");
                string message = result ? "人类胜利/humanwin" : "丧尸胜利/zombiewin";
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] {message}");
            };

            _zpApi.HZP_OnGameModeSelect += (string modename) =>
            {
                Core.Logger.LogInformation("模式选择触发/Mode selection trigger");
                Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 选择完毕模式名称为/After selecting the mode name {modename}");
            };
        }
    }

   

    public override void Load(bool hotReload)
    {

        Core.Command.RegisterCommand("sw_zpapi", zpapi, true); //检查游戏状态与 职业状态例子 /Examples of checking game status and class status

        Core.Command.RegisterCommand("sw_set", zpset, true); //切换人类和僵尸例子,直接设置/Switch between human and zombie examples, set directly.
        Core.Command.RegisterCommand("sw_setm", zpsetm, true); //设置为 特殊丧尸例子/Set to special zombie example
        Core.Command.RegisterCommand("sw_inf", zpinf, true); // 直接感染自己例子/Directly infect yourself example

        Core.Command.RegisterCommand("sw_setitem", zpsetitem, true); //给予t病毒疫苗与设置为人类特殊职业例子/Give TVaccine and Set to special human example
        Core.Command.RegisterCommand("sw_giveitem", zpgiveitem, true); // 给予各种物品例子/Give various items example

        Core.Command.RegisterCommand("sw_setwin", zpsetwin, true); //直接设置胜负例子/Directly set victory or defeat example

        Core.Command.RegisterCommand("sw_setglow", setglow, true); //直接设置玩家发绿光和fov例子/Directly set player to glow green and fov example

        Core.Command.RegisterCommand("sw_check", checkzombie, true); //获取当前丧尸属性快照 / Get current zombie attribute snapshot

    }

    
    
    
    public void checkzombie(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;
        var id = player.PlayerID;
        if (_zpApi.HZP_IsZombie(id))
        {
            var name = _zpApi.HZP_GetZombieClassname(player);
            var valve = _zpApi.HZP_GetZombieProperties(name);
            if (valve == null)
                return;

            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 名字/name{valve.Name}\n血/health{valve.Health}\n伤害/damage{valve.Damage}\n重力/gravity{valve.Gravity}\n速度/speed{valve.Speed}");
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 回血/EnableRegen{valve.EnableRegen}\n秒/sec{valve.HpRegenSec}\n量/hp{valve.HpRegenHp}\n母体血/motherzombie health{valve.MotherHealth}\n路径/modelpath{valve.ModelPath}");

        }

    }

    public void setglow(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        _zpApi.HZP_SetPlayerGlow(player, 0, 255, 0, 255);

        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 直接设置自己发绿光/set youself green glow");

        _zpApi.HZP_SetPlayerFov(player, 120);
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 直接设置自己120fov/ set youself fov 120");

    }

    public void zpsetwin(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        if (!int.TryParse(context.Args[0], out int count))
            return;

        if (count == 0)
        {
            _zpApi.HZP_SetHumanWin();
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 直接设置人类胜利/ set human win");
        }
        else if (count == 1)
        {
            _zpApi.HZP_SetZombieWin();
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 直接设置丧尸胜利/ set zombie win");
        }

    }

    public void zpgiveitem(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        if (!int.TryParse(context.Args[0], out int count))
            return;

        if (count == 0)
        {
            _zpApi.HZP_GiveTVirusGrenade(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给予T病毒手雷/Give T-virus grenade");
        }
        else if (count == 1)
        {
            _zpApi.HZP_GiveScbaSuit(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给予防化服/give sacb suit");
        }
        else if (count == 2)
        {
            _zpApi.HZP_GiveGodState(player, 30);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给予无敌模式 30 秒/ give god mode 30sec");
        }
        else if (count == 3)
        {
            _zpApi.HZP_GiveInfiniteAmmo(player, 30);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给予无限子弹 30 秒/ give GiveInfiniteAmmo 30sec");
        }
        else if (count == 4)
        {
            _zpApi.HZP_HumanAddHealth(player, 500);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给自己加500血/ add 500 health");
        }
        else if (count == 5)
        {
            _zpApi.HZP_GiveFireGrenade(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给自己燃烧手雷/ GiveFireGrenade");
        }
        else if (count == 6)
        {
            _zpApi.HZP_GiveLightGrenade(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给自己照明手雷/ GiveLightGrenade");
        }
        else if (count == 7)
        {
            _zpApi.HZP_GiveFreezeGrenade(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给自己冰冻手雷/GiveFreezeGrenade");
        }
        else if (count == 8)
        {
            _zpApi.HZP_GiveTeleportGrenade(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给自己传送手雷/GiveTeleportGrenade");
        }
        else if (count == 9)
        {
            _zpApi.HZP_GiveIncGrenade(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 给自己火焰燃烧弹/GiveIncGrenade");
        }


    }

    public void zpsetitem(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        if (!int.TryParse(context.Args[0], out int count))
            return;

        if (count == 0)
        {
            _zpApi.HZP_SetTargetTVaccine(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 直接给予t病毒解药/give T virus vaccine");
        }
        else if (count == 1)
        {
            _zpApi.HZP_SetTargetSniper(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为狙击手/ set yourself as a Sniper");
        }
        else if (count == 2)
        {
            _zpApi.HZP_SetTargetSurvivor(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为幸存者/ set yourself as a Survivor");
        }
        else if (count == 3)
        {
            _zpApi.HZP_SetTargetHero(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为英雄/ set yourself as a Hero");
        }


    }
    public void zpinf(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        var id = player.PlayerID;

        _zpApi.HZP_InfectPlayer(player, true);

        Core.Scheduler.DelayBySeconds(1.0f, () =>
        {

            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test]当前丧尸名字/zombie name {_zpApi.HZP_GetZombieClassname(player)}");
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test]当前最大血量从配置/get max health from cfg{_zpApi.HZP_GetZombieMaxHealth(player, true)}");
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test]当前最大血量从当前/get max health from pawn.maxhealth {_zpApi.HZP_GetZombieMaxHealth(player, false)}");
        });
    }

    public void zpset(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        var id = player.PlayerID;

        if (_zpApi.HZP_IsZombie(id))
        {
            _zpApi.HZP_SetTargetHuman(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为人类/Set yourself as a human");
        }
        else
        {
            _zpApi.HZP_SetTargetZombie(player);

            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为丧尸/Set yourself as a zombie");
        }


    }

    public void zpsetm(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        if (!int.TryParse(context.Args[0], out int count))
            return;

        if (count == 0)
        {
            _zpApi.HZP_InfectMotherZombie(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为母体丧尸/ Set yourself as a mother zombie");
        }
        else if (count == 1)
        {
            _zpApi.HZP_SetTargetNemesis(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为复仇之神/ Set yourself as a  Nemesis");
        }
        else if (count == 2)
        {
            _zpApi.HZP_SetTargetAssassin(player);
            Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 将自己设置为暗杀者/ Set yourself as a  Assassin");
        }
    }

    public void zpapi(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || !player.IsValid)
            return;

        if (_zpApi == null)
            return;

        var id = player.PlayerID;

        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 游戏开始状态/Game Start Status {_zpApi.GameStart}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是丧尸/Am I a zombie? {_zpApi.HZP_IsZombie(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是母体丧尸/Am I the mother zombie? {_zpApi.HZP_IsMotherZombie(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是复仇之神/Am I the god of vengeance? {_zpApi.HZP_IsNemesis(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是暗杀者/Am I an assassin? {_zpApi.HZP_IsAssassin(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是幸存者/Am I a survivor? {_zpApi.HZP_IsSurvivor(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是狙击手/Am I a sniper? {_zpApi.HZP_IsSniper(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是英雄/Am I a hero? {_zpApi.HZP_IsHero(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否有防化服/Do I have a scba suit? {_zpApi.HZP_PlayerHaveScbaSuit(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否处于无敌状态/Am I god mode? {_zpApi.HZP_PlayerHaveGodState(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 自己是否是无限子弹状态/Am I in a state of unlimited ammo? {_zpApi.HZP_PlayerHaveInfiniteAmmoState(id)}");
        Core.PlayerManager.SendMessage(MessageType.Chat, $"[ZPAPI测试/ZPAPI Test] 当前模式名字为/The current mode name is {_zpApi.HZP_GetCurrentModeName()}");
    }


    public override void Unload()
    {

    }


}
    
    
    
