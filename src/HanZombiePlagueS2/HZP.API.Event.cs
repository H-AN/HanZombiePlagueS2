
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Helpers;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using static HanZombiePlagueS2.HZPZombieClassCFG;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace HanZombiePlagueS2;

public partial class HanZombiePlagueAPI : IHanZombiePlagueAPI, IDisposable
{
    /*
     * 游戏开始事件
    */
    private Action<bool>? OnGameStart;

    public event Action<bool>? HZP_OnGameStart
    {
        add => OnGameStart += value;
        remove => OnGameStart -= value;
    }

    public void NotifyGameStart(bool gameStart)
    {
        OnGameStart?.Invoke(gameStart);
    }

    /*
     * 丧尸感染事件
    */
    private Action<IPlayer, IPlayer, bool, string>? OnPlayerInfect;

    public event Action<IPlayer, IPlayer, bool, string>? HZP_OnPlayerInfect
    {
        add => OnPlayerInfect += value;      
        remove => OnPlayerInfect -= value;   
    }

    public void NotifyInfect(IPlayer attacker, IPlayer victim, bool grenade, string name)
    {
        OnPlayerInfect?.Invoke(attacker, victim, grenade, name);
    }

    /*
     * 母体丧尸选择事件
    */
    private Action<IPlayer>? OnMotherZombieSelected;

    public event Action<IPlayer>? HZP_OnMotherZombieSelected
    {
        add => OnMotherZombieSelected += value;
        remove => OnMotherZombieSelected -= value;
    }

    public void NotifyMotherZombieSelected(IPlayer player)
    {
        OnMotherZombieSelected?.Invoke(player);
    }

    /*
     * 复仇之神丧尸选择事件
    */
    private Action<IPlayer>? OnNemesisSelected;

    public event Action<IPlayer>? HZP_OnNemesisSelected
    {
        add => OnNemesisSelected += value;
        remove => OnNemesisSelected -= value;
    }

    public void NotifyNemesisSelected(IPlayer player)
    {
        OnNemesisSelected?.Invoke(player);
    }

    /*
     * 暗杀者丧尸选择事件
    */
    private Action<IPlayer>? OnAssassinSelected;

    public event Action<IPlayer>? HZP_OnAssassinSelected
    {
        add => OnAssassinSelected += value;
        remove => OnAssassinSelected -= value;
    }

    public void NotifyAssassinSelected(IPlayer player)
    {
        OnAssassinSelected?.Invoke(player);
    }

    /*
     * 英雄玩家选择事件
    */
    private Action<IPlayer>? OnHeroSelected;

    public event Action<IPlayer>? HZP_OnHeroSelected
    {
        add => OnHeroSelected += value;
        remove => OnHeroSelected -= value;
    }

    public void NotifyHeroSelected(IPlayer player)
    {
        OnHeroSelected?.Invoke(player);
    }

    /*
     * 幸存者选择事件
    */
    private Action<IPlayer>? OnSurvivorSelected;

    public event Action<IPlayer>? HZP_OnSurvivorSelected
    {
        add => OnSurvivorSelected += value;
        remove => OnSurvivorSelected -= value;
    }

    public void NotifySurvivorSelected(IPlayer player)
    {
        OnSurvivorSelected?.Invoke(player);
    }

    /*
     * 狙击手选择事件
    */
    private Action<IPlayer>? OnSniperSelected;

    public event Action<IPlayer>? HZP_OnSniperSelected
    {
        add => OnSniperSelected += value;
        remove => OnSniperSelected -= value;
    }

    public void NotifySniperSelected(IPlayer player)
    {
        OnSniperSelected?.Invoke(player);
    }

    /*
     * 人类胜利广播
    */
    private Action<bool>? OnHumanWin;

    public event Action<bool>? HZP_OnHumanWin
    {
        add => OnHumanWin += value;
        remove => OnHumanWin -= value;
    }

    public void NotifyHumanWin(bool HumanWin)
    {
        OnHumanWin?.Invoke(HumanWin);
    }

    /*
     * 模式选择广播
    */
    private Action<string>? OnGameModeSelect;

    public event Action<string>? HZP_OnGameModeSelect
    {
        add => OnGameModeSelect += value;
        remove => OnGameModeSelect -= value;
    }

    public void NotifyGameModeSelect(string ModeName)
    {
        OnGameModeSelect?.Invoke(ModeName);
    }

    /*
     * 丧尸偏好选择操作广播
     * 当玩家用菜单选择了一个丧尸偏好后
     * 调用这个方法，触发事件
     * 让外部插件知道玩家改了偏好，可以存数据库
    */
    private Action<ulong, string?>? OnPreferenceChanged;

    public event Action<ulong, string?>? HZP_OnPreferenceChanged
    {
        add => OnPreferenceChanged += value;
        remove => OnPreferenceChanged -= value;
    }
    // 内部方法：当玩家点击菜单时调用

    public void NotifyUpdatePreferenceFromMenu(int slot, ulong steamId, string? newClassName)
    {
        var isRandom = string.IsNullOrEmpty(newClassName);
        var config = new ZombiePreferenceConfig
        {
            Preference = isRandom ? ZombiePreference.Random : ZombiePreference.Fixed,
            FixedZombieName = newClassName ?? string.Empty
        };

        _zombieState.PlayerPreferences[slot] = config;

        if (isRandom)
        {
            _zombieState.ExternalPreferences.Remove(steamId);
        }
        else
        {
            _zombieState.ExternalPreferences[steamId] = newClassName!;
        }

        OnPreferenceChanged?.Invoke(steamId, newClassName);
    }

}
